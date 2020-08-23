#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM registry.cn-hangzhou.aliyuncs.com/newbe36524/runtime-deps:3.1

#拷贝项目publish文件夹中的所有文件到 docker容器中的publish文件夹中 , publish 可自行更改名称

COPY . /supernat/client

#设置工作目录为 `/publish` 文件夹，即容器启动默认的文件夹

WORKDIR /supernat/client

#使用`dotnet xx.dll`来运行应用程序
ENTRYPOINT ["dotnet", "SuperNAT.Client.dll"]