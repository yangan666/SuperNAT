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
        title: '主机',
        controller: 'Client',
        showValue: 'name'
      },
      curd: {
        affterAdd: () => {
          this.getUserList()
        },
        affterGetOne: (item) => {

        }
      },
      columns: [
        {
          type: 'select',
          text: "所属用户",
          value: 'user_name',//表格显示的
          align: 'left',
          width: 100,
          sortable: false,
          table: true
        },
        {
          type: 'select',
          text: "所属用户",
          value: 'user_id',//表单下拉框
          form: true,
          items: [],
          itemText: 'user_name',
          itemValue: 'user_id',
          change: (id) => {
            
          },
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '请选择所属用户'
          }
        },
        {
          type: 'input',
          text: '主机名称',
          value: 'name',
          align: 'left',
          width: 160,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '主机名称不能为空'
          }
        },
        {
          type: 'input',
          text: '主机密钥',
          value: 'secret',
          align: 'left',
          width: 280,
          sortable: false,
          table: true
        },
        {
          type: 'input',
          text: '二级域名',
          value: 'subdomain',
          align: 'left',
          width: 160,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '二级域名不能为空'
          }
        },
        {
          type: 'input',
          text: '最后活动时间',
          value: 'last_heart_time',
          align: 'left',
          width: 180,
          sortable: false,
          table: true
        },
        {
          type: 'input',
          text: '创建时间',
          value: 'create_time',
          align: 'left',
          width: 180,
          sortable: false,
          table: true
        },
        {
          type: 'input',
          text: "描述",
          value: 'remark',
          align: 'left',
          width: 250,
          sortable: false,
          table: true,
          form: true
        },
        {
          type: 'tag',
          text: "状态",
          color: (item) => {
            return item.is_online ? 'success' : 'error'
          },
          value: 'is_online_str',
          align: 'left',
          width: 80,
          sortable: false,
          table: true
        },
        {
          type: 'action',
          text: "操作",
          align: 'left',
          sortable: false,
          table: true,
          form: false,
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
      ]
    }
  },
  methods: {
    //用户列表
    getUserList () {
      request({
        url: '/Api/User/GetList',
        method: 'post',
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.columns[1].items = data.Data
        }
      })
    }
  }
}
</script>