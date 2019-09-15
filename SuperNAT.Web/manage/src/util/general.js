import Vue from "vue"
window.Tools = {
    getCookieDomain: function () {
        var host = location.hostname;
        var ip = /^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$/;
        if (ip.test(host) === true || host === 'localhost') return host;
        var match = host.match('([^]*).*');
        if (typeof match !== "undefined" && null !== match) host = match[1];
        if (typeof host !== "undefined" && null !== host) {
            var strAry = host.split(".");
            if (strAry.length > 1) {
                host = strAry[strAry.length - 2] + "." + strAry[strAry.length - 1];
            }
        }
        return '.' + host;
    },//获取网站主域名
    setCookie: function (name, value, time = '1h') {
        function getsec(str) {
            var str1 = str.substring(1, str.length) * 1;
            var str2 = str.substring(0, 1);
            if (str2 === "s") {
                return str1 * 1000;
            }
            else if (str2 === "h") {
                return str1 * 60 * 60 * 1000;
            }
            else if (str2 === "d") {
                return str1 * 24 * 60 * 60 * 1000;
            }
        }
        var strsec = getsec(time);
        var exp = new Date();
        exp.setTime(exp.getTime() + strsec * 1);
        var host = Tools.getCookieDomain();
        document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString() + ";domain=" + host + ";path=/";
    },//设置cookie
    getCookie: function (name) {
        var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
        arr = document.cookie.match(reg)
        if (arr)
            return unescape(arr[2]);
        else
            return null;
    },//获取cookie
    delCookie: function (name) {
        var exp = new Date();
        exp.setTime(exp.getTime() - 1000);
        var cval = Tools.getCookie(name);
        if (cval !== null) {
            var host = Tools.getCookieDomain();
            document.cookie = name + "=" + cval + ";expires=" + exp.toGMTString() + ";domain=" + host + ";path=/";;
        }
    },//删除cookie
    getParam: function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //构造一个含有目标参数的正则表达式对象
        var r = decodeURIComponent(window.location.search).substr(1).match(reg);  //匹配目标参数
        if (r !== null) return unescape(r[2]); return null; //返回参数值
    },//获取url参数
    getSingleParam: function (index) {
        var paramStr = window.location.search.substr(1);
        var paramArr = paramStr.split('?');
        if (paramArr.length - 1 >= index)
            return paramArr[index];
        else
            return null;
    },//获取?分割的参数 zhongkewei.com?abd&passsword=sbs
    clearParam: function () {
        if (!cleanParam) return;
        var path = this.getUrlRelative();
        history.replaceState(null, '', path);
    },//清空url参数
    isNumber: function (obj) {
        var reg = /^[0-9]*$/;
        return reg.test(obj);
    },//判断是否数字
    checkMobile: function () {
        if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent))
            return true;
    },//检查是否手机页面
    getUrlRelative: function (saveParam) {
        var url = document.location.toString();
        var arrUrl = url.split("//");
        var start = arrUrl[1].indexOf("/");
        var relUrl = arrUrl[1].substring(start);//stop省略，截取从start开始到结尾的所有字符
        if (!saveParam && relUrl.indexOf("?") != -1) {
            relUrl = relUrl.split("?")[0];
        }
        return relUrl;
    },//获取相对路径
    compareDate: function (begintime, endtime) {
        var curTime = new Date();
        //把字符串格式转化为日期类
        var starttime = new Date(begintime);
        var endtime = new Date(endtime);
        //进行比较
        return starttime < endtime;
    },//比较日期大小
    downloadFile: function (src, name) {
        var eleId = 'downloadFile';
        var ele = document.querySelector('#' + eleId);
        if (!ele) {
            var template = `<a href="` + src + `" id="` + eleId + `" style="display:none;" download="` + name + `" title=>导出数据</a>`;
            document.body.insertAdjacentHTML('beforeEnd', template);
            ele = document.querySelector('#' + eleId);
        } else {
            ele.download = name;
            ele.href = src;
        }
        ele.click();
    },
    watchToExcute: function (callback, obj, propertyName, watchValue) {
        var interval = setInterval(function () {
            if (obj[propertyName] && ((watchValue && (JSON.stringify(obj[propertyName]) == JSON.stringify(watchValue))) || !watchValue)) {
                if (typeof callback == 'function') {
                    callback();
                    clearInterval(interval);
                }
            }
        }, 50);
    },//监听到值执行方法/监听对象/对象属性/监听目标值
    getFormatDate: function (day) {
            var day = day;
            var Year = 0;
            var Month = 0;
            var CurrentDate = "";
            //初始化时间 
            Year = day.getFullYear();//ie火狐下都可以 
            Month = day.getMonth() + 1;   
            CurrentDate += Year + "-";
            if (Month >= 10) {
                CurrentDate += Month + "-";
            }
            else {
                CurrentDate += "0" + Month;
            }       
            return CurrentDate;
    }//格式化时间为yyyy-mm形式
};

String.prototype.replaceAll = function (s1, s2) { return this.replace(new RegExp(s1, "gm"), s2); };//字符串增加[替换所有]方法
String.prototype.trimEnd = function (c) {
    if (c == null || c == "") {
        var str = this;
        var rg = /s/;
        var i = str.length;
        while (rg.test(str.charAt(--i)));
        return str.slice(0, i + 1);
    }
    else {
        var str = this;
        var rg = new RegExp(c);
        var i = str.length;
        while (rg.test(str.charAt(--i)));
        return str.slice(0, i + 1);
    }
}

Array.prototype.remove = function (target) {
    for (var i = 0, len = this.length; i < len; i++) {
        if (JSON.stringify(target) == JSON.stringify(this[i])) {
            this.splice(i, 1);
            break;
        }
    }
};//移除数组指定对象/值   arr.remove(5)

window.VTools = {
    _dialog: null,//模态窗对象
    _confirm: null,//对话窗对象
    _msg: null,//剧中提示对象
    _loading: null,//加载提示对象
    _hint: null,//底部提示对象
    showApp: function (app) {
        var id = '#' + app.$el.id;
        document.querySelector(id).style.visibility = 'visible';
    },//显示app,处理显示源码的不美观问题
    setUserInfo: function (vue) {
        vue.userInfo = {
            userid: Tools.getCookie('userid'),//数据操作目标账号
            //checkUser: Tools.getCookie('checkUser'),//数据操作人账号
            name: Tools.getCookie('name'),
            role: Tools.getCookie('role'),
            token: Tools.getCookie('token'),
            //position: Tools.getCookie('position'),
            //job: Tools.getCookie('job'),
            orgId: Tools.getCookie('orgId'),
            orgName: Tools.getCookie('orgName')
        }
    },//设置用户信息到Vue数据中
    pickFile: function (callback, accept, size = 10) {
        var id = 'mFile';
        var ele = document.querySelector('#' + id);
        if (ele)
            ele.parentNode.removeChild(ele);
        var fileEle = document.createElement("input");
        fileEle.type = "file";
        fileEle.name = "mFile";
        fileEle.id = id;
        fileEle.accept = accept || 'image/*';
        fileEle.style.display = "none";
        document.body.appendChild(fileEle);
        var ele = document.querySelector('#' + id);
        if (typeof callback === 'function') {
            ele.onchange = function () {
                var f = ele.files[0];
                var fileSize = (f.size / 1024 / 1024).toFixed(2);
                if (size < fileSize) {
                    VTools.msg('不能大于' + size + 'MB！当前' + fileSize + 'MB');
                    return;
                }//文件大小限制
                var _URL = window.URL || window.webkitURL;
                var file, img;
                if ((file = this.files[0])) {
                    img = new Image();
                    img.onload = function () {
                        $('.img').attr('src', this.src);
                    };
                    callback({
                        file: f,
                        src: _URL.createObjectURL(file)
                    });//返回
                }
            };
        }
        ele.click();
    },//选择文件
    uploadAttach: function (data) {
        var formData = new FormData();
        for (var d in data) {
            formData.append(d, data[d]);
        }
        return Ajax({
            url: 'WorkerManage/List/AddPersonalFile',
            method: 'POST',
            data: formData
        });
    },//上传用户附件
    uploadFile: function (url, data) {
        var formData = new FormData();
        for (var d in data) {
            formData.append(d, data[d]);
        }
        return Ajax({
            url: url,
            method: 'POST',
            data: formData
        });
    },//上传一般文件
    getType: function (typeId) {
        return Ajax({
            url: 'Home/TypeList/' + typeId
        });
    },//获取下拉框值
    showData: function (v_data, data, match) {
        try {
            if (data instanceof Array) {//数组类型
                v_data.splice(0, v_data.length);
                for (var i = 0; i < data.length; i++) {
                    for (var o in data[i]) {
                        data[i][o] = this.clearDeafault(data[i][o]);
                    }
                    v_data.push(data[i]);
                }
            } else {//对象类型
                for (var d in data) {
                    if (typeof v_data[d] !== 'undefined') {
                        v_data[d] = this.clearDeafault(data[d]);
                        if (match && typeof match[d] !== 'undefined' && v_data[d]) {
                            for (var item of match[d]) {
                                if (item.text === v_data[d]) {
                                    v_data[d] = "" + item.value;
                                    break;
                                }
                            }
                        }
                    } else {
                        console.log("未匹配到属性=>" + d + ":" + typeof v_data[d])
                    }
                }
            }
        }
        catch (e) {
            debugger
            console.log(e);
        }

    },//返回一个匹配值的对象,match转换下拉框
    showRequired: function (data, checkNames) {
        for (var objData in data) {
            if (checkNames) {
                for (var name in checkNames) {
                    if (objData == checkNames[name]) {
                        if (data[objData] === '')
                            return false;
                        break;
                    }
                }
            } else {
                if (data[objData] === '' || data[objData] === null || data[objData] === undefined)
                    return false;
            }
        }
        return data;
    },
    clearDeafault: function (str) {
        var normalStr = '';
        if (str instanceof Array) {//数组
            for (var i = 0, len = str.length; i < len; i++) {
                str[i] = this.clearDeafault(str[i]);
            }
        } else if (convertToNormal(str)) {//字符串
            str = normalStr;
        } else if (typeof str === 'object') {//对象
            for (var o in str) {
                str[o] = this.clearDeafault(str[o]);
            }
        }
        function convertToNormal(s) {
            var typeArr = ["@", "0001-01-01", "0001-01-01 00:00:00",
                "-1", "0000-00-00至0000-00-00", "0000-00-00"];
            if (typeof s == 'undefined' || s == null)
                return true;
            for (var i = 0; i < typeArr.length; i++) {
                if (typeArr[i] == s)
                    return true;
            }
            return false;
        }
        return str;
    },//清空默认值
    dialog: function (title, url, width, height, callback) {
        if (!document.querySelector('#modal')) {
            var template = `<v-dialog id="modal" v-model="isShow" scrollable persistent :width="width" style="display: none;">
                                <v-card>
                                    <v-card-title style="padding:0 0 0 16px;">
                                        <span class="headline" style="font-size:17px !important;">{{title}}</span>
                                        <v-spacer></v-spacer>
                                        <v-btn flat icon color="red" @click="close()">
                                          <v-icon>close</v-icon>
                                        </v-btn>
                                    </v-card-title>
                                    <v-divider></v-divider>
                                    <iframe id="ifr" name="ifr" frameborder="0" :src="url" marginwidth="0" marginheight="0" allowtransparency="true"></iframe>
                                </v-card>
                            </v-dialog>`;//字符串模板 @只用一个
            document.body.insertAdjacentHTML('beforeEnd', template);
        }

        if (!this._dialog) {
            this._dialog = new Vue({
                el: '#modal',
                data: {
                    title,
                    url,
                    width,
                    height,
                    isShow: false,
                    oIframe: null,
                    callback
                },
                methods: {
                    close: function () {
                        this.isShow = false;
                        this.oIframe.src = "";
                        if (typeof this.callback === 'function') {
                            this.callback();
                        }
                    }
                },
                mounted: function () {
                    this.oIframe = document.getElementById('ifr');
                    this.oIframe.style.height = this.height + 'px';
                }
            })
        } else {
            this._dialog.title = title;
            this._dialog.oIframe.src = url;
            this._dialog.width = width;
            this._dialog.height = height;
            this._dialog.oIframe.style.height = height + 'px';
            this._dialog.callback = callback;
        }
        this._dialog.oIframe.style.height = height + 'px';
        this._dialog.isShow = true;
    },//显示弹窗 用法: VTools.dialog('个人基本信息', '/Staff/ElementalModify', 500, 600, function () {});
    closeDialog: function (params) {
        if (!parent) return;
        if (typeof parent.VTools._dialog.callback === 'function')
            parent.VTools._dialog.callback(params);
        parent.VTools._dialog.isShow = false;
        parent.VTools._dialog.src = "";
    },//关闭弹窗 手动关闭可以传参
    confirm: function (params) {
        if (!document.querySelector('#confirm')) {
            var template = `<v-dialog id="confirm" v-model="isShow" persistent max-width="300px" style="display: none;" lazy>
                                <v-card>
                                    <v-card-title style="padding: 10px 10px 0 16px;">
                                        <span class="headline">{{title}}</span>
                                    </v-card-title>
                                    <v-card-text>{{content}}</v-card-text>
                                    <v-card-actions>
                                        <v-spacer></v-spacer>
                                        <v-btn color="mBlue darken-1" flat @click="confirm(2)">取消</v-btn>
                                        <v-btn color="mBlue darken-1" flat @click="confirm(1)">确定</v-btn>
                                    </v-card-actions>
                                </v-card>
                            </v-dialog>`;//字符串模板 @只用一个
            document.body.insertAdjacentHTML('beforeEnd', template);
        }

        if (!this._confirm) {
            this._confirm = new Vue({
                el: '#confirm',
                data: {
                    title: params.title,
                    content: params.content,
                    isShow: false,
                    cancel: params.cancel,
                    ensure: params.ensure,
                    backParam: params.backParam
                },
                methods: {
                    confirm: function (type) {
                        this.isShow = false;
                        if (type === 2) {
                            if (typeof this.cancel === 'function') {
                                this.cancel(this.backParam);
                            }
                        } else if (type === 1) {
                            if (typeof this.ensure === 'function') {
                                this.ensure(this.backParam);
                            }
                        }
                    }
                }
            });
        } else {
            this._confirm.title = params.title;
            this._confirm.content = params.content;
            this._confirm.cancel = params.cancel;
            this._confirm.ensure = params.ensure;
            this._confirm.backParam = params.backParam;
        }
        this._confirm.isShow = true;
    },//显示对话框 用法:VTools.confirm('个人基本信息', '内容', function () {});
    msg: function (content, duration, callback) {
        if (!document.querySelector('#msg')) {
            var template = `<v-dialog id="msg" v-model="isShow" persistent no-click-animation max-width="150px" style="display: none" lazy dark>
                                <v-card>
                                    <v-card-text class="text-md-center" style="color:white;text-align:center">{{content}}</v-card-text>
                                </v-card>
                            </v-dialog>`;//字符串模板
            document.body.insertAdjacentHTML('beforeEnd', template);
        }

        if (!this._msg) {
            this._msg = new Vue({
                el: '#msg',
                data: {
                    content,
                    isShow: false
                }
            })
        } else {
            this._msg.content = content;
        }
        this._msg.isShow = true;
        setTimeout(function () {
            VTools._msg.isShow = false;
            if (typeof callback === 'function')
                callback();
        }, duration || 2000);
    },//显示提示 用法:VTools.msg('这是提示信息');
    hint: function (text, timeout = 6000) {
        if (!document.querySelector('#hint')) {
            var template = `<v-snackbar id="hint" v-model="isShow" color="info" :multi-line="mode === 'multi-line'" :timeout="timeout" :vertical="mode === 'vertical'">
                                {{ text }}
                                <v-btn dark flat @click="isShow = false">关闭</v-btn>
                            </v-snackbar>`;
            document.body.insertAdjacentHTML('beforeEnd', template);
        }

        if (!this._hint) {
            this._hint = new Vue({
                el: '#hint',
                data: {
                    mode: text.length > 15 ? 'multi-line' : '',
                    timeout,
                    text,
                    isShow: false
                }
            });
        } else {
            this._hint.text = text;
            this._hint.timeout = timeout;
        }
        this._hint.isShow = true;


    },//底部提示
    loading: function (content) {
        if (!document.querySelector('#loading')) {
            var template = `<v-dialog id="loading" v-model="isShow" persistent width="180" lazy>
                                <v-card>
                                    <v-card-text >
                                        {{content?content:'加载中...'}}
                                        <v-progress-linear :indeterminate="true" style="margin-bottom:-5px" height="2"></v-progress-linear>
                                    </v-card-text>
                                </v-card>
                            </v-dialog>`;//字符串模板
            document.body.insertAdjacentHTML('beforeEnd', template);
        }

        if (!this._loading) {
            this._loading = new Vue({
                el: '#loading',
                data: {
                    content: content,
                    isShow: false
                }
            });
        } else {
            this._loading.content = content;
        }
        this._loading.isShow = true;
    },//显示加载中提示 用法:VTools.loading('请求数据中...');
    loaded: function () {
        if (this._loading) {
            this._loading.isShow = false;
        }
    },//隐藏加载提示 用法:VTools.loaded();
    getLeaf: function (tree, id) {
        for (var i = 0; i < tree.length; i++) {
            if (Number(tree[i].id) === Number(id))
                return tree[i];
            else if (tree[i].children && tree[i].children.length > 0) {
                var r = this.getLeaf(tree[i].children, id);
                if (r)
                    return r;
            }
        }
    },//获取节点
    getNameRootById: function (tree, id, attr) {
        for (var i = 0; i < tree.length; i++) {
            if (Number(tree[i].id) === Number(id))
                return tree[i][attr];
            else if (tree[i].children && tree[i].children.length > 0) {
                var attrVal = this.getNameRootById(tree[i].children, id, attr);
                if (attrVal)
                    return tree[i][attr] + '/' + attrVal;
            }
        }
    },//根据orgId获取节点从顶级节点到该节点的所有名称
    removeEmptyChildren: function (tree) {
        for (var i = 0; tree && i < tree.length; i++) {
            if (!tree[i].children || tree[i].children.length == 0)
                delete tree[i].children;
            else
                this.removeEmptyChildren(tree[i].children);
        }
    },//去除空树节点
    limitInput: function (regStr, event) {
        var reg = new RegExp(regStr, 'gi');
        var el = event.currentTarget;
        el.value = el.value.replace(reg, '');
        var maxLen = event.currentTarget.maxLength;
        if (maxLen) {
            el.value = el.value.substring(0, maxLen);
        }
    },//限制文本框输入;第一个参数:正则表达式(不允许输入的),第二个参数:触发对象
    filterHeader: function (header, nowId) {
        for (var i = header.length - 1; i >= 0; i--) {
            if (header[i].showOn && header[i].showOn instanceof Array) {
                var isShow = false;
                for (var id of header[i].showOn) {
                    if (id == nowId) {
                        isShow = true;
                        break;
                    }
                }
                !isShow && header.remove(header[i]);
            }
        }
    },//移除不需要显示的表头 参数:表头列表/当前页面标识码
    //clearUserid: function () {
    //    Tools.setCookie('userid', Tools.getCookie('checkUser'));
    //    Tools.setCookie('lookJob', Tools.getCookie('job'));
    //    Tools.delCookie('targetName');
    //},//清除查看账号
};