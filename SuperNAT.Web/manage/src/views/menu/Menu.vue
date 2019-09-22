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
        add: {
          affterAdd: () => { },
          success: (e) => {

          }
        }
      },
      columns: [
        {
          text: "名称",
          value: 'name',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required|max:10',
          counter: 0,
          requiredInfo: {
            required: () => '名称不能为空',
            max: '名称长度不能超过10位'
          }
        },
        {
          text: "描述",
          value: 'remark',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '描述不能为空'
          }
        },
        {
          type: 'action',
          text: "操作",
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: false,
          actions: [
            {
              name: '编辑',
              handle: (item) => {
                this.$refs.curd.edit(item)
              }
            },
            {
              name: '禁用',
              handle: (item) => {
                this.disable(item)
              }
            },
            {
              name: '删除',
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
    }
  }
}
</script>