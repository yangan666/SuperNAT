using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperNAT.Common
{
    /// <summary>
    /// 公共帮助类
    /// </summary>
    public static class UtilityTools
    {
        /// <summary>  
        /// 获取枚举描述
        /// </summary>  
        /// <param name="en">枚举</param>  
        /// <returns>返回枚举的描述 </returns>  
        public static string GetDescription(this Enum en)
        {
            Type type = en.GetType();   //获取类型  
            MemberInfo[] memberInfos = type.GetMember(en.ToString());   //获取成员  
            if (memberInfos != null && memberInfos.Length > 0)
            {
                //获取描述特性  
                if (memberInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attrs && attrs.Length > 0)
                {
                    return attrs[0].Description;    //返回当前描述
                }
            }
            return en.ToString();
        }

        /// <summary>
        /// 通过NetworkInterface获取MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMacByNetworkInterface()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                }
            }
            catch (Exception)
            {
            }
            return "00-00-00-00-00-00";
        }

        /// <summary>
        /// 将byte[]输出为图片
        /// </summary>
        /// <param name="path">输出图片的路径及名称</param>
        /// <param name="picByte">byte[]数组存放的图片数据</param>
        public static void SavePicFromBytes(string path, byte[] picByte)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            //开始写入
            bw.Write(picByte, 0, picByte.Length);
            //关闭流
            bw.Close();
            fs.Close();
        }

        /// <summary>
        /// 获取本机IP
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIp()
        {
            string localIp = NetworkInterface.GetAllNetworkInterfaces()
                            .Select(p => p.GetIPProperties())
                            .SelectMany(p => p.UnicastAddresses)
                            .FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address))?.Address.ToString();

            return localIp;
        }
    }
}
