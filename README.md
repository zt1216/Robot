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


9月12日开发日志

一、 刷怪器落地与 SpawnPoint 布置

EnemySpawner 挂到场景空物体后开验收，Inspector 数组槽里没有东西可拖——四个 SpawnPoint 得自己在场景里建。建了四个空物体（E/W/N/S 东南西北四个方向命名），Transform 摆到掩体外围四面边缘（大约正负 20），Y 等于 0 贴地。怪从场景边缘刷是有意设计：怪刷脸上玩家没反应时间等于白掉血，刷在边缘怪要走一段路，玩家有时间转视角换弹找掩体；四面受敌的紧张感正是波次生存游戏的压力来源，COD 僵尸模式、L4D、DOOM 全是玩家视野外刷怪的行业惯例。

SpawnPoint 摆完 Play，怪出来了但一帧爆 7 条 NRE——7 个怪同时 Update 报同一个错。堆栈指向 EnemyAI 的 player 字段没拖。想回 Prefab 编辑模式把场景里的 Player 拖进去，Unity 不让拖——学了一课：Prefab 是资产，资产不能引用场景里的对象，不然这个 Prefab 拖到别的场景就废了，这是 Unity 设计如此不是操作错误。运行时 Instantiate 出来的怪继承的是 Prefab 的状态，所以场景引用必须在运行时用代码找。修法是 EnemyAI 的 Start 里加自动找：GameObject.FindGameObjectWithTag("Player") 拿到再取 transform，注意是单数 Object 的那个（FindGameObjectsWithTag 复数版返回数组，又一个容易混的 API 对）。

二、 空格跳跃

PlayerController 加跳跃，三个新概念。一、controller.isGrounded：CharacterController 自带属性，脚下踩着东西就是 true，跳跃必须先判落地否则按住空格无限跳上天。二、垂直速度自己积分：水平移动每帧 Move 就完了，但跳跃是物理过程，起跳给向上初速度、每帧被重力往下拽、落地归零，要养一个 verticalVelocity 字段每帧变。三、跳跃初速度公式 v 等于根号下 2gh：verticalVelocity 等于 Mathf.Sqrt(jumpHeight 乘 -2f 乘 gravity)，跳 1.5 米重力 -20，套公式就是精确值。这个公式面试能聊。

实现里 isGrounded 分支先钳 verticalVelocity 等于 -1f 防负值累积越掉越快，Input.GetKeyDown(KeyCode.Space) 用 Down 不用 GetKey（Down 是按下那一帧，GetKey 是按住期间每帧 true，用错就按住空格连跳成仙）。空中分支 verticalVelocity 加 gravity 乘 Time.deltaTime 积分重力。最后把 verticalVelocity 塞进 moveDir 的 y 分量再 Move，TransformDirection 只有 yaw 旋转没有俯仰，y 分量过去还是 y 放心塞。

首版两处错：moveDir 拼成 moveDie、Sqrt 那行少分号，都是红波浪线级别的错等于三查第一查没走。改完过审。落地不抖不穿地、按住空格只跳一次、边跑边跳方向不歪，三件验收全过。jumpHeight 1.5 偏蹦床，TPS 建议降到 1.0 到 1.2 重力拉到 -25 到 -30 更利索，回头场景定型再调。

三、 Wave 计数 UI 与两个祖传 bug

EnemySpawner 的 wave 字段是 private 外部读不到，照 PlayerHealth 里 MaxHealth 的老路加了一行属性 public int CurrentWave 只读暴露。WaveHUD 脚本照抄 PlayerHUD 的哨兵套路：两个判空、取 CurrentWave、跟 lastShown 不一样才更新文字，int 直接用不等于判断不用浮点容差那套。

首版三处错。最重的一处是取错数据：写了 enemySpawner.toSpawn——toSpawn 是这波还剩几只没刷，不是第几波，而且它还是 private 编译直接挂。toSpawn、wave、AliveCount 三个数据各管一摊（波数、待刷数、场上存活数），拿错数据等于显示逻辑全错但一行红波浪线没有，这种错 IDE 抓不住只能靠自己读。另外两处小错：int 声明的哨兵塞了 -1f（整数家里放小数编译器不认），字符串 Wave 后面少空格显示成 Wave3 黏一块。

Wave UI 通了以后打完一波第二波不刷，排查发现 Hierarchy 里 6 只 Enemy(Clone) 全活着——不是波次逻辑坏，是有怪没死透。真凶是敌人死亡只把血归零没有销毁物体，尸体 Tag 还是 Enemy，FindGameObjectsWithTag 照样数进去，AliveCount 永远不等于零，喘息分支永远进不去。修法一行：扣血脚本血量小于等于零的分支最后加 Destroy(gameObject)。Destroy 放最后，它一跑物体直接消失，写在后面的代码等于没写。以后想要尸体躺一会儿的效果可以换协程 WaitForSeconds 再 Destroy，现在直接销毁就行。

修完整个闭环打通：移动全家桶、射击全家桶、敌人 AI、波次刷怪、Wave 计数、死亡重开。TPS 从技术 demo 变游戏的一天。commit 推 Gitee 中途老剧本连踩两坑：先忘了 cd 到项目目录在 system32 下跑 git（没 .git 当然炸），后远程领先 fetch first 老朋友 pull --rebase 解决。commit ab4056a5 落地。

四、 游戏外壳开工：主菜单与存档

功能闭环了该套壳。现有场景另存为 Assets/Scenes/GameScene 归位，新建 Empty 场景做 MainMenu（Empty 不带相机灯光——Canvas 用 Screen Space Overlay 直接画屏幕上不靠相机，留个相机空跑没必要）。搭 Canvas 加标题加开始和退出两个按钮，脚本两个方法：StartGame 里 SceneManager.LoadScene 进战斗场景，QuitGame 里 Application.Quit。Application.Quit 编辑器 Play 模式没反应是正常的，只对打包 exe 生效，想验证绑对了加句 Debug.Log。Build Settings 两个场景都加进列表、MainMenu 排第 0 位，漏了这步 LoadScene 直接报 Scene couldn't be loaded。首版多拼了一行 using UnityEngine.SenceManagement（SceneManagement 拼错，正确那行已经在上面了这行纯属多余），红波浪线又漏网。

存档系统选了进度档不是最高纪录：打到 Wave 5 点继续游戏直接从 Wave 5 接着打。核心问题是跨场景传数据——场景一切换之前的对象全没了，入门解法静态变量：static bool loadSave 挂在类本身上不属于任何物体，场景切换不销毁，类名直接点访问。开始游戏设 false 继续游戏设 true，两条路进同一个 GameScene。EnemySpawner 两个口子：Start 里读档（MainMenu.loadSave 为 true 时 wave 等于 PlayerPrefs.GetInt 的 SavedWave 减一，这样第一波 wave 加加以后正好从存档那波重打；外面包一层 saved 大于 0 的守卫防第一次玩出 Wave 0），StartWave 里写档（每波开刷瞬间 SetInt，波次推进就存，中途闪退不亏，这是随时存和死亡存的思路差别）。

这块三轮迭代暴露三个错。PlayerPreff 拼写不用补全根本拼不对；PlayerPrefs.GetInt 当 SetInt 用——想存钱做了个查询动作啥也没存进去，而且 GetInt 吐了返回值没接没空转，返回值必接的老病第 8 犯；最阴的是把两个方案都塞进 Start 互相打架——if 外面一行无条件 wave 等于存档减一，if 里面又一行 wave 等于存档原值，新开局也被存档污染。两分支方案只能选一个是单选题不是多选题。改完单一路径过审。

五、 死亡结算面板与主菜单背景

死亡面板有旧底子：GameOverManager 的 Show() 外部调用式触发（PlayerHealth 死了喊一声，比 Update 轮询血量干净，保留），R 键快捷重开也保留跟按钮并存。在旧版上补四样：波数显示（取 enemySpawner.CurrentWave 拼进你死了活到 Wave 几）、鼠标放出、回主菜单按钮、两个按钮的路由方法。

3D 特有的坑：PlayerController 的 Start 把鼠标锁了藏了，死亡面板弹出来鼠标还锁在屏幕中间，按钮根本点不了干瞪眼。所以 Show() 里必须 Cursor.lockState 等于 CursorLockMode.None 加 Cursor.visible 等于 true 把鼠标放出来。首版把这行写反成了 Locked（继续锁死），记法是进游戏锁出弹窗放，一来一回互逆操作。判空守卫也补上——Show() 开头三连判空，这个守卫有个副作用：哪个槽没拖，保护了不崩但也把没挂引用从 NRE 爆炸变成静默失败，死不弹面板排查时先看四个槽有没有值。当晚真就踩了这个：新槽没拖，Show() 一进来撞判空 return，面板的 SetActive 根本执行不到。拖上就通。

主菜单背景太黑不好看，生成了一张末日废土低多边形风格的背景图（黄昏色调上半截天空留白正好压标题）。接入四步：图拖进 Assets（建 Textures 文件夹归置），Texture Type 改成 Sprite (2D and UI) 再 Apply（UI 的 Image 只吃 Sprite 不改这步拖不进 Source Image），Canvas 下建 Image 按住 Alt 点 stretch-stretch 铺满全屏，最后把 Background 在 Hierarchy 里拖到最前面垫底——UI 渲染顺序就是 Hierarchy 顺序，后建的 Image 插在最后会盖住标题和按钮。

当天收尾 commit 推送，pull --rebase 被本地未暂存改动拦住（编辑器里又改了点东西没进 Git），一行流解法 git pull --rebase --autostash：自动 stash 未暂存改动、拉远程、rebase、自动 pop 回来。

六、 小结

一天干了两天的量。跳跃补完移动全家桶，Wave UI 给波次加了节奏反馈，尸体销毁修复打通刷怪闭环，晚上游戏外壳一口气完成大半：主菜单三按钮、存档系统（静态变量跨场景传旗子加 PlayerPrefs 随波写档）、死亡结算面板（战果显示加鼠标放出）、主菜单背景图。新知识点：静态变量跨场景、CursorLockMode 进出配对、Autostash。外壳就剩暂停一块砖。
