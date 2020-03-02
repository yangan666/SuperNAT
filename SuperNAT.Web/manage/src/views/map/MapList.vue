<template>
  <simple-curd ref="curd" :basic="basic" :curd="curd" :columns="columns" />
</template>
<script>
import SimpleCurd from "../curd/SimpleCurd"
import request from "@/util/request"
export default {
  components: {
    SimpleCurd
  },
  data() {
    var is_admin = this.$store.getters.user.is_admin
    return {
      basic: {
        title: "映射",
        controller: "Map",
        showValue: "name",
        dialogWith: 600
      },
      curd: {
        affterAdd: () => {
          this.getUserList()
          this.getClientList()
          this.getProtocolList()
          this.getProxyTypeList()
        },
        affterGetOne: item => {
          this.selectUserChange(item.user_id)
          // this.selectProtocolChange(item.protocol)
          this.getServerConfig()
        }
      },
      columns: [
        {
          type: "select",
          text: "所属用户",
          value: "user_name", //表格显示的
          align: "left",
          width: 100,
          sortable: false,
          table: is_admin
        },
        {
          type: "select",
          text: "所属用户",
          value: "user_id", //表单下拉框
          form: is_admin,
          formRowXs: is_admin ? 6 : 12,
          items: [],
          itemText: "user_name",
          itemValue: "user_id",
          change: id => {
            this.selectUserChange(id)
          },
          validate: "required",
          requiredInfo: {
            required: () => "请选择所属用户"
          }
        },
        {
          type: "select",
          text: "协议类型",
          value: "protocol",
          align: "left",
          width: 100,
          sortable: false,
          table: true,
          form: true,
          formRowXs: is_admin ? 6 : 12,
          items: [],
          itemText: "Value",
          itemValue: "Value",
          change: val => {
            // this.selectProtocolChange(val)
          },
          validate: "required",
          requiredInfo: {
            required: () => "请选择协议类型"
          }
        },
        {
          type: "select",
          text: "主机名称",
          value: "client_name", //表格显示的
          align: "left",
          width: 150,
          sortable: false,
          table: true
        },
        {
          type: "select",
          text: "主机名称",
          value: "client_id", //表单下拉框
          form: true,
          formRowXs: 6,
          items: [],
          itemText: "name",
          itemValue: "id",
          change: id => {},
          validate: "required",
          requiredInfo: {
            required: () => "请选择主机"
          }
        },
        {
          type: "input",
          text: "应用名称",
          value: "name",
          align: "left",
          width: 150,
          sortable: false,
          table: true,
          form: true,
          formRowXs: 6,
          validate: "required",
          requiredInfo: {
            required: () => "应用名称不能为空"
          }
        },
        {
          type: "input",
          text: "内网主机",
          value: "local",
          form: true,
          formRowXs: 6,
          validate: "required",
          requiredInfo: {
            required: () => "内网地址不能为空"
          }
        },
        {
          type: "input",
          text: "内网端口",
          value: "local_port",
          form: true,
          formRowXs: 6,
          validate: "required",
          requiredInfo: {
            required: () => "内网端口不能为空"
          }
        },
        {
          type: "input",
          text: "内网主机",
          value: "local_endpoint",
          align: "left",
          width: 150,
          sortable: false,
          table: true
        },
        {
          type: "input",
          text: "外网域名",
          value: "remote",
          form: true,
          formRowXs: 6,
          validate: "required",
          requiredInfo: {
            required: () => "外网地址不能为空"
          }
        },
        {
          type: "input",
          text: "外网端口",
          value: "remote_port",
          form: true,
          formRowXs: 6,
          validate: "required",
          requiredInfo: {
            required: () => "外网端口不能为空"
          }
        },
        {
          type: "select",
          text: "代理类型",
          value: "proxy_type", //表单下拉框
          form: true,
          formRowXs: 12,
          items: [],
          itemText: "Value",
          itemValue: "Key",
          change: id => {},
          validate: "required",
          requiredInfo: {
            required: () => "请选择主机"
          }
        },
        {
          type: "input",
          text: "访问地址",
          value: "remote_endpoint",
          align: "left",
          width: 150,
          sortable: false,
          table: true
        },
        {
          type: "switch",
          text: "加密传输",
          value: "is_ssl",
          align: "left",
          width: 120,
          sortable: false,
          table: false,
          textFormat: ({ is_ssl }) => {
            return is_ssl ? "是" : "否"
          },
          form: false,
          formRowXs: 6,
          change: val => {
            // this.selectIsSsl(val)
          },
          validate: "required",
          requiredInfo: {
            required: () => "请选择是否加密传输"
          }
        },
        {
          type: "input",
          text: "证书文件",
          value: "certfile",
          form: false,
          formRowXs: 6
          // validate: 'required',
          // requiredInfo: {
          //   required: () => '请选择证书文件'
          // }
        },
        {
          type: "input",
          text: "证书密码",
          value: "certfile",
          form: false,
          formRowXs: 6
          // validate: 'required',
          // requiredInfo: {
          //   required: () => '请填写证书密码'
          // }
        },
        {
          type: "tag",
          text: "主机状态",
          color: item => {
            return item.is_online ? "success" : "error"
          },
          value: "is_online_str",
          align: "left",
          width: 100,
          sortable: false,
          table: true
        },
        {
          type: "alert",
          text: "开放端口",
          value: "server_config",
          align: "left",
          form: true
        },
        {
          type: "action",
          text: "操作",
          align: "left",
          width: 200,
          sortable: false,
          table: true,
          actions: [
            {
              name: item => {
                return "编辑"
              },
              handle: item => {
                this.getUserList()
                this.getClientList()
                this.getProtocolList()
                this.getProxyTypeList()
                this.$refs.curd.edit(item)
              }
            },
            {
              name: item => {
                return "删除"
              },
              handle: item => {
                this.$refs.curd.del(item)
              }
            }
          ]
        }
      ],
      clientList: []
    }
  },
  methods: {
    //开放端口
    getServerConfig() {
      request({
        url: "/Api/ServerConfig/GetServerConfig",
        method: "post",
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.$refs.curd.formItem.server_config = data.Data
        }
      })
    },
    //用户列表
    getUserList() {
      request({
        url: "/Api/User/GetList",
        method: "post",
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.columns[1].items = data.Data
        }
      })
    },
    //主机列表
    getClientList() {
      request({
        url: "/Api/Client/GetList",
        method: "post",
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.clientList = data.Data
          if (!this.$store.getters.user.is_admin) {
            this.columns[4].items = data.Data.filter(c => c.user_id == this.$store.getters.user.user_id)
          }
        }
      })
    },
    //获取协议列表
    getProtocolList() {
      request({
        url: "/Api/Common/GetEnumList?type=protocol",
        method: "post",
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.columns[2].items = data.Data
        }
      })
    },
    //获取枚举
    getProxyTypeList() {
      request({
        url: "/Api/Common/GetEnumList?type=proxy_type",
        method: "post",
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.columns[11].items = data.Data
        }
      })
    },
    //选择所属用户事件
    selectUserChange(id) {
      this.$refs.curd.formItem.client_id = null
      if (id) {
        //根据用户id过滤主机名称数据源
        if (this.$store.getters.user.is_admin) {
          var items = this.clientList.filter(c => c.user_id == id)
          this.columns[4].items = items
        }
      } else {
        //清空主机名称数据源
        this.columns[4].items = []
      }
    }
    // selectProtocolChange (val) {
    //   if (val == "http") {
    //     //协议栏占一行 隐藏证书文件 证书密码 345
    //     this.columns[this.columns.length - 3].form = false
    //     this.columns[this.columns.length - 4].form = false
    //     this.columns[this.columns.length - 5].form = false
    //     this.columns[this.columns.length - 6].formRowXs = 12
    //   } else if (val == "https") {
    //     //协议栏占一行 隐藏证书文件 证书密码
    //     this.columns[this.columns.length - 3].form = true
    //     this.columns[this.columns.length - 4].form = true
    //     this.columns[this.columns.length - 5].form = false
    //     this.columns[this.columns.length - 6].formRowXs = 12
    //   } else {
    //     //tcp udp
    //     this.columns[this.columns.length - 3].form = false
    //     this.columns[this.columns.length - 4].form = false
    //     this.columns[this.columns.length - 5].form = true
    //     this.columns[this.columns.length - 6].formRowXs = 12

    //     this.selectIsSsl(this.$refs.curd.formItem.is_ssl)
    //   }
    // },
    // selectIsSsl (val) {
    //   this.columns[this.columns.length - 3].form = val
    //   this.columns[this.columns.length - 4].form = val
    //   this.columns[this.columns.length - 6].formRowXs = val ? 6 : 12
    // }
  }
}
</script>
