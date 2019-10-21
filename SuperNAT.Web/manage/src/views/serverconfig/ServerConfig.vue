<template>
  <simple-curd ref="curd"
               :basic="basic"
               :curd="curd"
               :columns="columns" />
</template>
<script>
import SimpleCurd from '../curd/SimpleCurd'
import request from "@/util/request"
export default {
  components: {
    SimpleCurd
  },
  data () {
    var is_admin = this.$store.getters.user.is_admin
    return {
      basic: {
        title: '映射',
        controller: 'ServerConfig',
        showValue: 'protocol',
        dialogWith: 500
      },
      curd: {
        affterAdd: () => {

        },
        affterGetOne: (item) => {
          // this.selectProtocolChange(item.protocol)
        }
      },
      columns: [
        {
          type: 'select',
          text: "协议类型",
          value: 'protocol',
          align: 'left',
          width: 100,
          sortable: false,
          table: true,
          form: true,
          items: ['http', 'https', 'tcp', 'udp'],
          change: (val) => {
            // this.selectProtocolChange(val)
          },
          validate: 'required',
          requiredInfo: {
            required: () => '请选择协议类型'
          }
        },
        {
          type: 'input',
          text: '监听端口',
          value: 'name',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true,
          validate: 'required',
          requiredInfo: {
            required: () => '监听端口不能为空'
          }
        },
        {
          type: 'switch',
          text: '加密传输',
          value: 'is_ssl',
          align: 'left',
          width: 120,
          sortable: false,
          table: false,
          textFormat: ({ is_ssl }) => {
            return is_ssl ? "是" : "否"
          },
          form: false,
          formRowXs: 6,
          change: (val) => {
            // this.selectIsSsl(val)
          },
          validate: 'required',
          requiredInfo: {
            required: () => '请选择是否加密传输'
          }
        },
        {
          type: 'input',
          text: '证书文件',
          value: 'certfile',
          form: false,
          formRowXs: 6,
          // validate: 'required',
          // requiredInfo: {
          //   required: () => '请选择证书文件'
          // }
        },
        {
          type: 'input',
          text: '证书密码',
          value: 'certfile',
          form: false,
          formRowXs: 6,
          // validate: 'required',
          // requiredInfo: {
          //   required: () => '请填写证书密码'
          // }
        },
        {
          type: 'tag',
          text: "状态",
          color: (item) => {
            return item.is_disabled ? 'error' : 'success'
          },
          value: 'is_disabled_str',
          align: 'left',
          width: 100,
          sortable: false,
          table: true
        },
        {
          type: 'action',
          text: "操作",
          align: 'left',
          width: 200,
          sortable: false,
          table: true,
          actions: [
            {
              name: (item) => {
                return '编辑'
              },
              handle: (item) => {
                this.$refs.curd.edit(item)
              }
            },
            {
              name: (item) => {
                return '删除'
              },
              handle: (item) => {
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