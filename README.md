# SuperNAT
SuperNAT是基于NET Core 3.0开源跨平台的内网穿透程序，使用SuperSokcet实现核心功能，功能类似花生壳，可用于穿透内网web应用，微信公众号本地调试等，目前仅支持http穿透，tcp穿透会在后续加入，SuperNAT.Server为服务端，SuperNAT.Client为客户端。<br/>
目前属于开发阶段，大家可自行注册免费使用，SuperNAT管理后台：http://www.supernat.cn:8088
![Image text](https://github.com/yangan666/SuperNAT/blob/master/Img/manage.png)
<br/>
<br/>
下面是window部署教程，linux可用docker进行部署，这里不做说明<br/>
SuperNAT.Server服务端部署：<br/>
第一步：先下载net core 3.0运行时，地址：https://dotnet.microsoft.com/download/dotnet-core/3.0 选择下载Runtime 3.0.0 ASP.NET Core/.NET Core: Runtime & Hosting Bundle并安装到服务器。<br/><br/>
第二步：下载源码生成SuperNAT.Server或者到release下载最新的安装包，修改配置文件appsettings.json的配置<br/><br/>
![Image text](https://github.com/yangan666/SuperNAT/blob/master/Img/server.config.png)
第三步：安装mariadb或mysql，数据库脚本位置：SuperNAT.Server/DB，创建数据库supernat，先执行脚本create.sql，然后执行data.sql，修改SuperNAT.Server配置文件appsettings.json的DBConfig>ConnetionString为你的数据库连接字符串。<br/><br/>
第四步：启动SuperNAT.Server控制台程序，浏览器打开http://你的域名:8088，默认管理员：admin，密码：123456<br/><br/>
第五步：内网穿透>主机管理>新建主机，生成主机密钥，然后在端口映射创建你的应用。<br/><br/>
控制台程序可以一键安装为系统服务，解压安装包后找到 安装.bat 点击后第一步会先安装为系统服务，完成出现提示后按回车启动，使用 卸载.bat 卸载服务<br/><br/>

SuperNAT.Client客户端部署：<br/>
第一步：先下载net core 3.0运行时，地址：https://dotnet.microsoft.com/download/dotnet-core/3.0 选择下载Runtime 3.0.0 ASP.NET Core/.NET Core: Runtime & Hosting Bundle并安装到电脑。<br/>
第二步：下载源码生成SuperNAT.Client或者到release下载最新的安装包，修改配置文件appsettings.json的配置，启动SuperNAT.Client即可完成内网映射<br/><br/>
![Image text](https://github.com/yangan666/SuperNAT/blob/master/Img/client.config.png)<br/><br/>
控制台程序可以一键安装为系统服务，解压安装包后找到 安装.bat 点击后第一步会先安装为系统服务，完成出现提示后按回车启动，使用 卸载.bat 卸载服务<br/><br/>
穿透示例：
![Image text](https://github.com/yangan666/SuperNAT/blob/master/Img/demo.png)<br/><br/>
已发布测试版，大家可到release下载体验，客户端可使用SuperNAT服务器的配置：<br/>
{<br/>
  "Secret": "你的主机密钥",<br/>
  "ServerUrl": "www.supernat.cn",<br/>
  "ServerPort": "8088",<br/>
  "NatPort": "10006"//报文传输监听端口<br/>
}<br/>
服务器开放的http端口有80,10000-10005

