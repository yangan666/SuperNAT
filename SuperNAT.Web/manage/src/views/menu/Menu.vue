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
        title: '菜单',
        controller: 'Menu',
        showValue: 'name'
      },
      curd: {
        affterAdd: () => {
          this.getMenuList()
        },
        affterGetOne: (item) => {

        }
      },
      columns: [
        {
          type: 'select',
          text: "上级菜单",
          value: 'pid',//表单下拉框
          form: true,
          items: [],
          itemText: 'title',
          itemValue: 'menu_id',
          change: (id) => {

          }
        },
        {
          type: 'input',
          text: '菜单标题',
          value: 'title',
          align: 'left',
          width: 120,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '菜单标题不能为空'
          }
        },
        {
          type: 'select',
          text: "上级菜单",
          value: 'p_title',//表格显示的
          align: 'left',
          width: 100,
          sortable: false,
          table: true
        },
        {
          type: 'input',
          text: '菜单图标',
          value: 'icon',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true
        },
        {
          type: 'input',
          text: '路由名称',
          value: 'name',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '路由名称不能为空'
          }
        },
        {
          type: 'input',
          text: '路由地址',
          value: 'path',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '路由地址不能为空'
          }
        },
        {
          type: 'input',
          text: '组件路径',
          value: 'component',
          align: 'left',
          width: 150,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '组件路径不能为空'
          }
        },
        {
          type: 'input',
          text: '排序',
          value: 'sort_no',
          align: 'left',
          width: 100,
          sortable: false,
          table: true,
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '排序不能为空'
          }
        },
        {
          type: 'switch',
          text: '是否隐藏',
          value: 'hidden',
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '请选择隐藏或显示'
          }
        },
        {
          type: 'switch',
          text: '是否隐藏',
          value: 'hidden_str',
          table: true,
          align: 'left',
          width: 120,
          sortable: false,
        },
        {
          type: 'switch',
          text: '总是显示',
          value: 'always_show',
          form: true,
          required: true,
          validate: 'required',
          requiredInfo: {
            required: () => '请选择总是显示或总是隐藏'
          }
        },
        {
          type: 'switch',
          text: '总是显示',
          value: 'always_show_str',
          table: true,
          align: 'left',
          width: 120,
          sortable: false,
        },
        {
          type: 'action',
          text: "操作",
          align: 'left',
          width: 200,
          sortable: false,
          table: true,
          form: false,
          actions: [
            {
              name: (item) => {
                return '编辑'
              },
              handle: (item) => {
                this.getMenuList()
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
    //菜单列表
    getMenuList () {
      request({
        url: `/Api/${this.basic.controller}/GetParentList`,
        method: 'post',
        data: {
          pid: ''
        }
      }).then(({ data }) => {
        if (data.Result) {
          this.columns[0].items = data.Data
        }
      })
    }
  }
}
</script>