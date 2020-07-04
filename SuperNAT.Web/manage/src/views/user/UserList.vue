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
    return {
      basic: {
        title: '用户',
        controller: 'User',
        showValue: 'user_name'
      },
      curd: {
        affterAdd: () => {
          this.getRoleList()
          this.columns[1].text = '密码，不填写默认123456'
        },
        affterGetOne: (item) => {

        }
      },
      columns: [
        {
          type: 'input',
          text: "用户名",
          value: 'user_name',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true,
          validate: 'required|max:15',
          counter: 15,
          requiredInfo: {
            required: () => '用户名不能为空',
            max: '用户名长度不能超过15位'
          }
        },
        {
          type: 'password',
          text: '密码',
          value: 'password',
          sortable: false,
          form: true
        },
        {
          type: 'input',
          text: "微信",
          value: 'wechat',
          align: 'left',
          width: 150,
          sortable: false,
          table: false,
          form: false
        },
        {
          type: 'input',
          text: "邮箱",
          value: 'email',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true,
          validate: 'required|email',
          requiredInfo: {
            required: () => '邮箱不能为空'
          }
        },
        {
          type: 'input',
          text: "手机号",
          value: 'tel',
          align: 'left',
          width: 180,
          sortable: false,
          table: false,
          form: false
        },
        {
          type: 'input',
          text: "角色",
          value: 'role_name',
          align: 'left',
          width: 150,
          sortable: false,
          table: true
        },
        {
          type: 'select',
          text: "角色",
          value: 'role_id',
          align: 'left',
          width: 100,
          form: true,
          items: [],
          itemText: 'name',
          itemValue: 'role_id',
          change: (id) => {

          },
          validate: 'required',
          requiredInfo: {
            required: () => '请选择角色'
          }
        },
        {
          type: 'input',
          text: "登录次数",
          value: 'login_times',
          align: 'left',
          width: 100,
          sortable: false,
          table: true
        },
        {
          type: 'input',
          text: "最后登录时间",
          value: 'last_login_time',
          align: 'left',
          width: 200,
          sortable: false,
          table: true
        },
        {
          type: 'input',
          text: "创建时间",
          value: 'create_time',
          align: 'left',
          width: 200,
          sortable: false,
          table: true
        },
        {
          type: 'tag',
          text: "状态",
          color: (item) => {
            return item.is_disabled ? 'error' : 'success'
          },
          value: 'is_disabled_str',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: false
        },
        {
          type: 'action',
          text: "操作",
          align: 'left',
          sortable: false,
          table: true,
          actions: [
            {
              name: (item) => {
                return '编辑'
              },
              handle: (item) => {
                this.$refs.curd.edit(item)
                this.getRoleList()
                this.columns[1].text = '密码，不修改无需填写'
              }
            },
            {
              name: (item) => {
                return item.is_disabled ? "启用" : "禁用"
              },
              handle: (item) => {
                this.disable(item)
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
      ]
    }
  },
  methods: {
    //禁用/启用
    disable (item) {
      this.$dialog.warning({
        text: `确定${item.is_disabled ? '启用' : '禁用'}${this.basic.title}"${item[this.basic.showValue]}"吗`,
        title: '提示',
        persistent: true,
        actions: {
          true: {
            color: 'primary',
            text: '确定',
            handle: () => {
              request({
                url: `/Api/${this.basic.controller}/Disable`,
                method: 'post',
                data: item
              }).then(({ data }) => {
                if (data.Result) {
                  this.$refs.curd.getList()
                  this.$dialog.message.success(data.Message, {
                    position: 'top'
                  })
                } else {
                  this.$dialog.message.error(data.Message, {
                    position: 'top'
                  })
                }
              })
            }
          },
          false: '取消'
        }
      })
    },
    //角色列表
    getRoleList () {
      request({
        url: '/Api/Role/GetList',
        method: 'post',
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.columns[6].items = data.Data
        }
      })
    },
  }
}
</script>