9月7日开发日志

一、项目启动
确定 TPS 项目方向：末日废墟题材，一个敌人配三状态 AI，整个项目预计 4 到 8 周完成。当天完成项目立项和 Gitee 远程仓库搭建，仓库地址 gitee.com/zt051216/tps，用户独立完成 Git 的 init、add、commit、push 全流程推送，第一次 push 失败后学会用 ls-remote 诊断远程仓库状态。

二、阶段一：移动与相机，通关
实现玩家前后左右移动、视角左右旋转由 Player 负责、视角上下俯仰由 CameraController 负责，俯仰角限制在正负 60 度。集成 CharacterController 解决穿墙问题，修复了转向 90 度后按 W 朝世界坐标方向走的朝向 bug。处理了光标锁定问题，确认编辑器 Game 视图焦点机制的特性。鼠标锁定通过 Cursor.lockState 实现。

三、阶段二：射击系统，全套通关
一天干完八项：准星、射线检测加命中特效、靶子扣血销毁、射速冷却、弹药数量与换弹、后坐力、枪声。核心技术包括 ScreenPointToRay 加 Raycast 射线检测，GetComponent 三步曲（接住、判空、调用），换弹协程，后坐力用单协程两段 while 实现平滑抬升与回弹，抬升每帧 0.2 回弹每帧 0.05，形成抬快回慢的真实手感。枪声部分理清了 AudioClip 是数据、AudioSource 是播放器的关系，用 PlayOneShot 播放。

四、当天修复的典型问题
后坐力抽搐（抬升瞬移与回弹平滑节奏不均，改为两段平滑）、换弹协程先补弹后等待导致瞬间换弹完成、协程叠加重入、多处引用没拖导致的空引用。当天战斗系统雏形全部完成。



9月8日开发日志

一、导航环境搭建
排查发现引擎缺少 AI Navigation 包导致找不到 Navigation 窗口，直接修改 manifest.json 添加 com.unity.ai.navigation 依赖装包成功。完成 NavMesh 烘焙，可行走区域和障碍物挖空识别正确。

二、敌人 AI 三状态机，四步干完三步
第一步追人脚本，NavMeshAgent 加 SetDestination 实现追踪移动目标和自动绕障碍。第二步巡逻与追击两状态机，切换阈值采用滞回设计，15 米内进入追击，20 米外放弃回到巡逻，防止临界距离来回抖动。第三步攻击态骨架，玩家贴近 3 米敌人停下刹车进入攻击姿态，跑出 8 米恢复追击，形成三态闭环。第四步攻击态盯人，锁 Y 轴 LookAt 实现水平瞄准，进入攻击时关闭 updateRotation 防止寻路组件抢转向控制权，退出时归还，敌人攻击时原地转身始终面向玩家。

三、当天修复的三个经典 bug
巡逻点随机出界：随机采样不认识地图边界，用 SamplePosition 以 20 米半径把点吸附回 NavMesh，失败则下帧自动重选。巡逻到点站桩：寻路组件在离目标 1.5 米处刹车，而代码要求 1 米内才算到达，两套参数不一致导致永不换点，阈值改为 2 米修复。后坐力橡皮筋（回头修射击系统）：射击后转视角被拽回开枪瞬间的角度，根因是协程启动时拍下的角度快照在玩家实时修改后失效，修法是玩家一碰鼠标立即 yield break 放弃控制权，同时补上开枪瞬间盲区的检查，并用 StopCoroutine 防止连发时多个后坐力协程叠加互相拉扯。

四、当前状态
敌人已具备自主巡逻、发现玩家追击、贴脸攻击姿态、玩家逃脱后恢复追击或巡逻的完整行为闭环。待补：攻击开火与伤害结算、敌人受击与死亡。



9月9日开发日志

一、 PlayerShooting 三个性能坑

排查 PlayerShooting 早期版发现三个埋雷点。第一是 sfx 每帧 GetComponent AudioSource（虽然没崩但每帧查询是浪费），提到 Start 缓存到字段，Update 只调 PlayOneShot。第二是 ammoText.text 每帧无脑赋值（文本脏标记触发 Mesh 重排版，量大后帧率肉眼掉），引入 ammoDirty 标志位，只在 Shoot 完成和 Reload 协程结束时置 true 才刷。第三是 Raycast 不传 LayerMask，子弹能打穿墙、还能打到自己 Layer 上的物体，按阶段②的设计引入 LayerMask：玩家身上勾三层（Player+Enemy+Obstacle）、敌人身上勾两层（Player+Obstacle 不勾自己防自伤）。两边勾的差异踩了三次才拍板：玩家要看子弹被队友挡、敌人不需要找自己。

顺带扫到一个 CS0108 警告，字段名撞了 Component.audio 保留字。Shadowing 是合法但不推荐，四联动改 sfx（PlayerShooting 四个引用点 + Inspector 不会丢引用因为字段名同义）。

 二、 阶段③真伤害闭环

接 EnemyAttack 调原型。NavMeshAgent 取玩家距离，3 米内进入攻击态，每隔 Attack Cooldown 秒扣玩家 5 血。EnemyHealth 同步写：受伤扣血、堆到 0 停追 + 找战斗组件引用销毁。

中间踩了一个离谱的坑，误改 Attack Cooldown=0，玩家一秒被打成筛子秒死，回查调回 1。

敌人随机攻击间隔由固定 2 秒改成 Random.Range(1, 5)，打破节拍器让玩家来不及预判。

 三、 阶段④死亡闭环开搞

GameOverManager 接死亡面板，默认 SetActive(false)，监听 R 键重启 SceneManager.LoadScene 当前场景名。PlayerHealth.Die() 三件套：关 PlayerController + 关 PlayerShooting + Show GameOver。第一版拆成三独立 if，敌人还会动但人死了，留半成品。修法是 && 短路原子操作：三个组件同时非 null 才执行，否则 Debug.LogError 打印缺组件名。多步骤一个动作必须 atomic，老病 8.5 犯再记一笔。

UI 三件套 Canvas / GameOverPanel / GameOverText 搭起来：Panel 默认隐藏，血条挂另外的 Text。

四、 阶段④血条 PlayerHUD 的四个雷

PlayerHUD 用轮询模式（每帧读 CurrentHealth，变化超阈值才刷文字）零耦合，框架跟 PlayerHealth 解耦。写完三查扫出四个雷：

1. 类名 vs 实例名：Update 里 PlayerHealth.Xxx 大写写成蓝图名了，应该用 Inspector 拖进来的实例 playerHealth.Xxx，老病 10 犯第二撞。
2. 差值没 Abs：玩家加血的时候 current > lastShown，差值出负的，Mathf.Abs 包一下才不漏刷。
3. Start 预设哨兵堵首刷：原本想在 Start 里 lastShown = MaxHealth，结果开局正好相等不再刷血条。删 Start 那一行，回退哨兵值 -1f 强制首刷。
4. 缺 RoundToInt：浮点直接拼接出 89.99998/100，两边套 Mathf.RoundToInt，肉眼干净。

五、 受伤红闪 DamageFlash 与一段接近崩溃的下午

第一版全屏红闪想糊脸上让玩家"我疼"。Image 组件 + 改颜色 alpha，0.35 起步 0.15 秒褪，过审。挂上去 Console 报 Can't add script + No MonoBehaviour scripts in the file，搞了足足两小时。

排查走偏了：第一猜文件名/类名不匹配（老病 11 犯），截图显示两边都对。真凶不在报错指向处——报错只说"这个类找不到"，但 C# 项目一损俱损，任何文件编译挂都拖全项目下水。第二步开 Console，揪到 EnemyAI 里改字段名改了半截：typo maxAttackCoolDOwn 字段名还残留着，两处必须联动改，改了一个 git 没自动改另一个。



9月10日开发日志

一、 红闪改版：屏幕边缘闪

全屏红闪 0.35 alpha 糊一脸太丑，调研改屏幕边缘闪（画框式血晕），符合射击游戏惯例——不挡准星，余光里出红。实施：DamageFlash 父物体挂 CanvasGroup（管总闸 alpha 0~1 小数），4 条边子 Image（纯红 alpha=255 不动，明暗全交给总闸）。脚本从 Image.color 改成 CanvasGroup.alpha，值对比轮询逻辑零改动——三查过审。

顺手把协程从啪→灭升级成渐退：while 减，起点 alpha 由代码显式设，否则首版 while 条件初始就 false 一次不闪（首版就中了这招）。衰减速度 2f 乘 Time.deltaTime，约 0.4 秒退完，while 退出后补 cg.alpha = 0 兜底钳零防浮点残留。

二、 边缘闪排障长跑

第一关 Console 报 Can't add script + No MonoBehaviour scripts in the file，老病 11 犯先猜文件名类名不匹配，截图核对两边都对。真凶不在报错指向处——报错只说类找不到，但 C# 项目一损俱损，任何文件编译挂都拖全项目下水。开 Console 一查，EnemyAI 里字段名 typo maxAttackCoolDOwn 改了一半，两处必须联动改，改一个另一个留着。修完编译过。

第二关 DamageFlash 物体 Hierarchy 复选框没勾，代码对、组件在、Console 干净，就是不跑。失活等于默认不做事，连 Inspector 报错都没有，最阴的一种坑。勾上物体，又发现 CanvasGroup 组件根本没挂在物体上——脚本 GetComponent 拿 null，协程第一行 NRE。加上组件。

第三关挨刀 alpha 数值从 0 到 0.8 到掉到 0，逻辑层全通，但画面就是不红。手动拉 alpha=1 也看不见。排查四轮：

1. 色板 A 误认：一直在拧 Image 色板的 A（0~~255），而脚本控的是 CanvasGroup 的 Alpha（0~~1 小数），两个旋钮名字都叫 alpha 但范围不同。CanvasGroup 总闸（0~~1）vs 每张图分闸色板 A（0~~255），分闸=255、总闸控明暗。找对旋钮后终于能拉了。
2. 父物体坐标系：四条边"贴边锚点"贴的是 DamageFlash 父物体的边，但父物体是默认 middle-center 100x100 缩在屏幕中央一小块。子物体锚点相对父的 RectTransform 边界算——子贴的也是那 100x100 的小框，全挤中间。修法：父物体 Shift 加 Alt 点 stretch-stretch 铺满全屏，四条边各自 Shift 加 Alt 对应边的 preset（top-stretch、bottom-stretch、left-stretch、right-stretch），锚点加 Pivot 加 Pos 三件一键归位。之前 EdgeLeft Pos X 等于负 383 这种离谱值，Shift 加 Alt 一按全清零。
3. 死亡状态盖住：测试时玩家已死，GameOverPanel 激活了，有张底图压在 DamageFlash 上面，活着测试就没事。活的时候手动拉 alpha 等于 1 看 Game 视图四周，红框出来了。

整理下来"脚本没跑"三级排查链：一、 Console 红字（项目编译挂）；二、 物体名旁复选框（GameObject 失活，组件全静默）；三、 组件前复选框（组件被独立关掉）。今天三关各踩一遍，这套排查图谱齐了。

UI 嵌套布局铁律：先铺满父、再搭子。

三、 边缘闪还是丑，改径向血晕

四条边画框虽然帅了，但厚度 100 像素加 0.8 透明度还是糊脸，挡瞄准。调研 AAA 射击游戏全用径向血晕（blood vignette）：一张"中心全透明、往四角渐变泛红"贴图，挨打时视野边缘渗血，中间画面干净，DOOM、使命召唤都是这个套路。

让 AI 用 Python 手写 PNG chunk 生成了 DamageVignette.png：1024x1024 中心透明、边缘渐变、深血红。接入：删四条边、DamageFlash 下挂单 Image、Shift 加 Alt stretch-stretch 铺满、Source Sprite 拖入贴图、Color 保持白色（红已经烧在贴图里）。CanvasGroup 和脚本零改动——值对比轮询解耦分红的又一次验证，UI 长什么样跟脚本怎么判断完全分离。

代码峰值 alpha 从 0.8 降到 0.5f，血晕是渐变的天然比色块柔和。


9月11日开发日志

课多忙不过来了，写了一点记入9月12开发中
