# SuperNAT
SuperNAT是基于.NET Core 3.1开源跨平台的内网穿透程序，功能类似花生壳，可用于穿透内网web应用，微信公众号本地调试等，目前支持http穿透，tcp穿透。目前属于开发阶段，大家可自行注册免费使用，SuperNAT管理后台：http://www.supernat.cn
<br/>
<br/>
SuperNAT.Server服务端部署：<br/>
（1）window部署教程：<br/>
第一步：先下载安装net core 3.1最新版运行时到服务器<br/><br/>
第二步：下载源码生成SuperNAT.Server或者到release下载最新的安装包，修改配置文件appsettings.json的配置<br/><br/>
第三步：安装mariadb或mysql，数据库脚本位置：SuperNAT.Server/DB，创建数据库supernat，先执行脚本create.sql，然后执行data.sql，修改SuperNAT.Server配置文件appsettings.json的DBConfig>ConnetionString为你的数据库连接字符串。<br/><br/>
第四步：启动SuperNAT.Server控制台程序，API接口默认8088端口，管理后台前端文件为SuperNAT\SuperNAT.Web\manage，可自行使用npm run build命令打包放到SuperNAT.Server的wwwroot目录（不存在文件夹就创建一个），修改appsettings.json的ServerPort端口，浏览器打开http://你的域名:ServerPort端口 也可以单独部署前端文件到其它端口（比如80端口），需要修改SuperNAT\SuperNAT.Web\manage\src\util\request.js的baseURL为API接口地址即可，管理后台默认管理员：admin，密码：123456<br/><br/>
第五步：系统管理>服务配置>新建配置，添加你的http、tcp转发端口（监听端口格式为211,2020,10001-10004,60009），添加好后重启服务。<br/><br/>
第六步：内网穿透>主机管理>新建主机，生成主机密钥，然后在端口映射创建你的应用。<br/><br/>
控制台程序可以一键安装为系统服务，解压安装包后找到 安装.bat 点击后第一步会先安装为系统服务，完成出现提示后按回车启动，使用 卸载.bat 卸载服务<br/><br/>
（2）Linux docker部署教程：<br/>
#拉取镜像<br/>
docker pull yangan666/supernat-server<br/>
#上传你自己的配置文件到/mnt/supernat/server/appsettings.json覆盖容器的配置（挂载配置文件位置可自定义），使用以下命令运行容器，注意mysql需要自行安装<br/>
docker run --name supernat-server -v /mnt/supernat/server/appsettings.json:/supernat/server/appsettings.json -itd --restart=always --network=host --log-opt max-size=50m --log-opt max-file=3 yangan666/supernat-server<br/>
#打开管理后台，新建服务配置，重启服务，创建你的映射，步骤同Windows教程<br/>

SuperNAT.Client客户端部署：<br/>
（1）window部署教程：<br/>
第一步：先下载安装net core 3.1最新版运行时到内网电脑<br/><br/>
第二步：下载源码生成SuperNAT.Client或者到release下载最新的安装包，修改配置文件appsettings.json的配置，启动SuperNAT.Client即可完成内网映射<br/><br/>

控制台程序可以一键安装为系统服务，解压安装包后找到 安装.bat 点击后第一步会先安装为系统服务，完成出现提示后按回车启动，使用 卸载.bat 卸载服务<br/><br/>
（2）Linux docker部署教程：<br/>
#拉取镜像<br/>
docker pull yangan666/supernat-client<br/>
#上传你自己的配置文件到/mnt/supernat/client/appsettings.json覆盖容器的配置（挂载配置文件位置可自定义），使用以下命令运行容器<br/>
docker run --name supernat-client -v /mnt/supernat/client/appsettings.json:/supernat/client/appsettings.json -itd --restart=always --network=host --log-opt max-size=50m --log-opt max-file=3 yangan666/supernat-client<br/>
#打开管理后台，新建服务配置，重启服务，创建你的映射，步骤同Windows教程<br/>

已发布测试版，大家可到release下载体验，客户端可使用SuperNAT服务器的配置：<br/>
{<br/>
  "Secret": "您的主机密钥",<br/>
  "ServerUrl": "www.supernat.cn",//服务器域名或IP地址<br/>
  "NatPort": "10006"//服务端报文传输监听端口<br/>
}<br/>
服务器开放的端口有：<br/>http 211,2020,10001-10004,60009<br/>https 10005<br/>tcp 10007-10020<br/><br/>
QQ交流群：854594944<br/>

