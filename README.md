# 说明
## 前期准备
1. SRanipaRuntime SDK（VIVE Eye and Facial Tracking SDK）：
下载地址：https://developer-express.vive.com/resources/vive-sense/eye-and-facial-tracking-sdk/download/latest/

2. SR_runtime软件：
steam搜索 VIVE Console for SteamVR
下载后在该软件安装目录下 \common\VIVEDriver\App\SRanipal 位置
<img src="images/屏幕截图 2024-07-18 114148.png" width="50%" />

## 使用：
1. unity中导入SDK：
在unity项目中import下图的package文件
<img src="images/屏幕截图 2024-07-18 114516.png" width="50%" />

2. Asset目录下多出一个ViveSR文件夹，要在自己的场景中使用，需要将该文件夹下的SRanipal Eye Framework预制体加入到场景中。
<img src="images/屏幕截图 2024-07-18 115044.png" width="50%" />
<img src="images/屏幕截图 2024-07-18 115708.png" width="20%" />
<img src="images/屏幕截图 2024-07-18 115729.png" width="20%" />

3. 将EyeDataCollect.cs文件挂载在vr相机上