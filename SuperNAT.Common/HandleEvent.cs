using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common
{
    public class HandleEvent
    {
        public static Action<int, Map> MapAction;
        public static void ChangeMap(int type, Map map)
        {
            MapAction?.Invoke(type, map);
        }
    }

    public enum ChangeMapType
    {
        新增 = 1,
        修改 = 2,
        删除 = 3
    }
}
