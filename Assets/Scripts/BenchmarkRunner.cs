using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 性能 A/B 实测脚本（v2，只用于测量，不参与玩法）
///
/// 测量原理：
///   阶段开始 → 强制 GC → 记录基线堆大小 → 一口气生成 N 组对象并「保持存活」
///   → 再次强制 GC → 记录堆大小 → 差值即为这 N 组对象的真实净分配
///   （用 GetTotalMemory(true)：先回收垃圾，再读「还活着的数据」占多大）
///   同时用 Stopwatch 记录这段生成调用的纯 CPU 耗时。
///
///   A 阶段：直造直毁（Instantiate）—— 复现优化前的生成路径
///   B 阶段：对象池（ObjectPool.Get）—— 当前实现
///
/// 结果：Console 打印 + 写入 项目根目录/bench_result.txt
/// 用法：空物体挂本脚本，拖 3 个 prefab + Canvas，按 Play（站空地上别动，建议关掉 EnemySpawner）
/// </summary>
public class BenchmarkRunner : MonoBehaviour
{
    [Header("引用（拖场景里正在用的那几个 prefab）")]
    public GameObject ballPrefab;          // 弹孔 BulletHole
    public GameObject tracerPrefab;        // 光弹 Tracer
    public GameObject damagePopupPrefab;   // 伤害飘字
    public RectTransform canvasRect;       // HUD 的 Canvas RectTransform

    [Header("参数")]
    public int objectsPerPhase = 30;       // 每阶段生成多少「组」（每组 = 弹孔+飘字+光弹）
    public bool autoRun = true;

    private struct PhaseResult
    {
        public string label;
        public long allocBytes;     // 净分配（字节）
        public double totalMs;      // 生成全部对象的纯 CPU 耗时
        public int objectCount;     // 实际对象数
    }

    private PhaseResult directResult;
    private PhaseResult pooledResult;
    private readonly List<GameObject> alive = new List<GameObject>();

    void Start()
    {
        if (autoRun)
        {
            StartCoroutine(RunAll());
        }
    }

    IEnumerator RunAll()
    {
        if (ballPrefab == null || tracerPrefab == null || damagePopupPrefab == null || canvasRect == null)
        {
            Debug.LogError("[Bench] 引用没拖全（ball / tracer / popup / canvasRect），测量中止");
            yield break;
        }
        if (ObjectPool.Instance == null)
        {
            Debug.LogError("[Bench] 场景里没有 ObjectPool，池化阶段无法测");
            yield break;
        }

        Debug.Log("[Bench] 2 秒后开始，期间请别开枪、别移动");

        yield return new WaitForSeconds(2f);
        yield return MeasureDirect();
        yield return new WaitForSeconds(1.5f);
        yield return MeasurePooled();

        PrintAndSave();
    }

    // ———————————— 阶段 A：直造直毁（优化前路径） ————————————
    IEnumerator MeasureDirect()
    {
        // 预热：让着色器/材质/JIT 先跑几轮，避免首轮虚高
        for (int i = 0; i < 5; i++)
        {
            SpawnSetDirect();
            CleanupAll();
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        System.GC.Collect();
        yield return null;
        System.GC.Collect();
        yield return null;

        long before = System.GC.GetTotalMemory(true);

        alive.Clear();
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < objectsPerPhase; i++)
        {
            SpawnSetDirect();
        }
        sw.Stop();

        long after = System.GC.GetTotalMemory(true);

        directResult = new PhaseResult
        {
            label = "优化前（Instantiate 直造直毁）",
            allocBytes = after - before,
            totalMs = sw.Elapsed.TotalMilliseconds,
            objectCount = alive.Count
        };

        CleanupAll();
        yield return null;
        yield return null;
    }

    // ———————————— 阶段 B：对象池（优化后） ————————————
    IEnumerator MeasurePooled()
    {
        // 提前把池子填够，保证 Get 全部命中缓存、不会偷偷 Instantiate
        ObjectPool.Instance.Prewarm(ballPrefab, objectsPerPhase + 10);
        ObjectPool.Instance.Prewarm(damagePopupPrefab, objectsPerPhase + 10);
        ObjectPool.Instance.Prewarm(tracerPrefab, objectsPerPhase + 10);
        yield return new WaitForSeconds(0.5f);

        System.GC.Collect();
        yield return null;
        System.GC.Collect();
        yield return null;

        long before = System.GC.GetTotalMemory(true);

        alive.Clear();
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < objectsPerPhase; i++)
        {
            SpawnSetPooled();
        }
        sw.Stop();

        long after = System.GC.GetTotalMemory(true);

        pooledResult = new PhaseResult
        {
            label = "优化后（对象池复用）",
            allocBytes = after - before,
            totalMs = sw.Elapsed.TotalMilliseconds,
            objectCount = alive.Count
        };

        ReturnAll();
        yield return null;
    }

    // 一组 = 弹孔 + 飘字 + 光弹，保持存活（不调用 FlyTo，避免自动回收）
    private void SpawnSetDirect()
    {
        Vector3 pos = transform.position + transform.forward * 5f;
        pos += Random.insideUnitSphere * 0.3f;

        GameObject hole = Instantiate(ballPrefab, pos, Quaternion.identity);
        alive.Add(hole);

        GameObject pop = Instantiate(damagePopupPrefab, canvasRect);
        DamagePopup dp = pop.GetComponent<DamagePopup>();
        if (dp != null)
        {
            dp.SetWorldPos(pos);
            dp.Init(34);
        }
        alive.Add(pop);

        GameObject tr = Instantiate(tracerPrefab, pos, Quaternion.identity);
        alive.Add(tr);
    }

    private void SpawnSetPooled()
    {
        Vector3 pos = transform.position + transform.forward * 5f;
        pos += Random.insideUnitSphere * 0.3f;

        GameObject hole = ObjectPool.Instance.Get(ballPrefab, pos, Quaternion.identity, 0f);
        alive.Add(hole);

        GameObject pop = ObjectPool.Instance.Get(damagePopupPrefab, pos, Quaternion.identity, 0f);
        pop.transform.SetParent(canvasRect, false);
        DamagePopup dp = pop.GetComponent<DamagePopup>();
        if (dp != null)
        {
            dp.SetWorldPos(pos);
            dp.Init(34);
        }
        alive.Add(pop);

        GameObject tr = ObjectPool.Instance.Get(tracerPrefab, pos, Quaternion.identity, 0f);
        alive.Add(tr);
    }

    private void CleanupAll()
    {
        for (int i = 0; i < alive.Count; i++)
        {
            if (alive[i] != null)
            {
                Destroy(alive[i]);
            }
        }
        alive.Clear();
    }

    private void ReturnAll()
    {
        for (int i = 0; i < alive.Count; i++)
        {
            if (alive[i] != null && alive[i].activeSelf)
            {
                ObjectPool.Release(alive[i]);
            }
        }
        alive.Clear();
    }

    private void PrintAndSave()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== TPS 性能 A/B 实测（v2 净分配法） =====");
        sb.AppendLine("时间: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("设备: " + SystemInfo.deviceModel + " / " + SystemInfo.graphicsDeviceName);
        sb.AppendLine("样本: 每组 = 弹孔+飘字+光弹，共 " + objectsPerPhase + " 组（" + (objectsPerPhase * 3) + " 个对象）");
        sb.AppendLine("方法: 强制 GC 后对比堆净增量（GetTotalMemory(true)），并用 Stopwatch 计纯 CPU 耗时");
        sb.AppendLine();

        AppendPhase(sb, directResult);
        AppendPhase(sb, pooledResult);
        sb.AppendLine("----------------------------------------");

        if (directResult.allocBytes > 0)
        {
            double perObjDirect = directResult.allocBytes / 1024.0 / Mathf.Max(1, directResult.objectCount);
            double perObjPooled = pooledResult.allocBytes / 1024.0 / Mathf.Max(1, pooledResult.objectCount);
            double drop = (1.0 - (double)pooledResult.allocBytes / directResult.allocBytes) * 100.0;

            sb.AppendLine(string.Format("单个对象 GC 分配 : {0:F2} KB → {1:F2} KB", perObjDirect, perObjPooled));
            sb.AppendLine(string.Format("本批 GC 分配总量 : {0:F1} KB → {1:F1} KB，下降 {2:F1}%",
                directResult.allocBytes / 1024.0, pooledResult.allocBytes / 1024.0, drop));

            if (pooledResult.totalMs > 0.001)
            {
                double perSpawnDirect = directResult.totalMs / Mathf.Max(1, objectsPerPhase) * 1000.0;
                double perSpawnPooled = pooledResult.totalMs / Mathf.Max(1, objectsPerPhase) * 1000.0;
                sb.AppendLine(string.Format("每发生成耗时     : {0:F1} μs → {1:F1} μs（快 {2:F1} 倍）",
                    perSpawnDirect, perSpawnPooled, perSpawnDirect / perSpawnPooled));
            }
            sb.AppendLine(string.Format("本批生成总耗时   : {0:F2} ms → {1:F2} ms", directResult.totalMs, pooledResult.totalMs));
        }
        sb.AppendLine("========================================");

        string text = sb.ToString();
        Debug.Log(text);

        try
        {
            string path = Application.dataPath + "/../bench_result.txt";
            System.IO.File.WriteAllText(path, text, System.Text.Encoding.UTF8);
            Debug.Log("[Bench] 结果已写入: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Bench] 写文件失败: " + e.Message);
        }
    }

    private void AppendPhase(StringBuilder sb, PhaseResult r)
    {
        sb.AppendLine("【" + r.label + "】");
        sb.AppendLine(string.Format("  净 GC 分配   : {0:F1} KB", r.allocBytes / 1024.0));
        sb.AppendLine(string.Format("  单个对象平均 : {0:F2} KB", r.allocBytes / 1024.0 / Mathf.Max(1, r.objectCount)));
        sb.AppendLine(string.Format("  生成总耗时   : {0:F2} ms（{1} 个对象）", r.totalMs, r.objectCount));
        sb.AppendLine(string.Format("  单次生成耗时 : {0:F1} μs",
            r.totalMs / Mathf.Max(1, objectsPerPhase) * 1000.0));
        sb.AppendLine();
    }
}
