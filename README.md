# SuperNAT
SuperNAT目前属于开发阶段，大家可自行注册免费使用，SuperNAT管理后台：http://www.supernat.cn
![Image text](https://github.com/yangan666/SuperNAT/blob/master/Img/manage.png)
<br/>
<br/>
下面是window部署教程，linux可用docker进行部署，这里不做说明<br/>
SuperNAT.Server服务端部署：<br/>
第一步：先下载安装net core 3.1最新版运行时到服务器<br/><br/>
第二步：下载源码生成SuperNAT.Server或者到release下载最新的安装包，修改配置文件appsettings.json的配置<br/><br/>
![Image text](https://github.com/yangan666/SuperNAT/blob/master/Img/server.config.png)
第三步：安装mariadb或mysql，数据库脚本位置：SuperNAT.Server/DB，创建数据库supernat，先执行脚本create.sql，然后执行data.sql，修改SuperNAT.Server配置文件appsettings.json的DBConfig>ConnetionString为你的数据库连接字符串。<br/><br/>
第四步：启动SuperNAT.Server控制台程序，API接口默认8088端口，管理后台前端文件为SuperNAT\SuperNAT.Web\manage，可自行使用npm run build命令打包放到SuperNAT.Server的wwwroot目录（不存在文件夹就创建一个），修改appsettings.json的ServerPort端口，浏览器打开http://你的域名:ServerPort端口 也可以单独部署前端文件到其它端口（比如80端口），需要修改SuperNAT\SuperNAT.Web\manage\src\util\request.js的baseURL为API接口地址即可，管理后台默认管理员：admin，密码：123456<br/><br/>
第五步：内网穿透>主机管理>新建主机，生成主机密钥，然后在端口映射创建你的应用。<br/><br/>
控制台程序可以一键安装为系统服务，解压安装包后找到 安装.bat 点击后第一步会先安装为系统服务，完成出现提示后按回车启动，使用 卸载.bat 卸载服务<br/><br/>

SuperNAT.Client客户端部署：<br/>
第一步：先下载安装net core 3.1最新版运行时到内网电脑<br/><br/>
第二步：下载源码生成SuperNAT.Client或者到release下载最新的安装包，修改配置文件appsettings.json的配置，启动SuperNAT.Client即可完成内网映射<br/><br/>
![Image text](https://github.com/yangan666/SuperNAT/blob/master/Img/client.config.png)<br/><br/>
控制台程序可以一键安装为系统服务，解压安装包后找到 安装.bat 点击后第一步会先安装为系统服务，完成出现提示后按回车启动，使用 卸载.bat 卸载服务<br/><br/>
穿透示例：
![Image text](https://github.com/yangan666/SuperNAT/blob/master/Img/demo.png)<br/><br/>
已发布测试版，大家可到release下载体验，客户端可使用SuperNAT服务器的配置：<br/>
{<br/>
  "Secret": "您的主机密钥",<br/>
  "ServerUrl": "www.supernat.cn",<br/>
  "ServerPort": "8088",<br/>
  "NatPort": "10006"//报文传输监听端口<br/>
}<br/>
服务器开放的端口有：<br/>http 211,2020,10001-10004,60009<br/>https 10005<br/>tcp 10007-10020<br/><br/>
QQ交流群：854594944<br/>

