<template>
  <div class="list-table">
    <v-container grid-list-xl
                 fluid>
      <v-layout row
                wrap>
        <v-flex lg12>
          <v-card>
            <v-toolbar card
                       color="white">
              <v-text-field flat
                            solo
                            prepend-icon="search"
                            placeholder="请输入关键词"
                            v-model="search"
                            hide-details
                            class="hidden-sm-and-down"></v-text-field>
              <v-dialog v-model="dialog"
                        persistent
                        max-width="500px">
                <template v-slot:activator="{ on }">
                  <v-btn color="primary"
                         dark
                         class="mb-2"
                         v-on="on"
                         @click="add">新建用户</v-btn>
                </template>
                <v-card>
                  <v-card-title>
                    <span class="headline">{{ formTitle }}</span>
                  </v-card-title>

                  <v-card-text>
                    <v-container grid-list-md>
                      <form>
                        <v-flex>
                          <v-text-field clearable
                                        v-model="formItem.user_name"
                                        v-validate="'required|max:15'"
                                        :counter="15"
                                        :error-messages="errors.collect('user_name')"
                                        data-vv-name="user_name"
                                        required
                                        label="用户名"></v-text-field>
                        </v-flex>
                        <v-flex>
                          <v-text-field clearable
                                        v-model="formItem.password"
                                        label="密码"
                                        type="password"></v-text-field>
                        </v-flex>
                        <v-flex>
                          <v-text-field clearable
                                        v-model="formItem.wechat"
                                        label="微信"></v-text-field>
                        </v-flex>
                        <v-flex>
                          <v-text-field clearable
                                        v-model="formItem.tel"
                                        label="手机"></v-text-field>
                        </v-flex>
                      </form>
                    </v-container>
                  </v-card-text>

                  <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="blue darken-1"
                           flat
                           @click="save">保存</v-btn>
                    <v-btn color="blue darken-1"
                           flat
                           @click="close">取消</v-btn>
                  </v-card-actions>
                </v-card>
              </v-dialog>
            </v-toolbar>
            <v-divider></v-divider>
            <v-card-text class="pa-0">
              <v-data-table :headers="table.headers"
                            :search="search"
                            :items="table.items"
                            :rows-per-page-items="table.pageSizes"
                            class="elevation-1"
                            item-key="name"
                            hide-actions>
                <template slot="items"
                          slot-scope="props">
                  <td class="text-xs-left">{{ props.item.user_name }}</td>
                  <td class="text-xs-left">{{ props.item.wechat }}</td>
                  <td class="text-xs-left">{{ props.item.tel }}</td>
                  <td class="text-xs-left">
                    <v-btn flat
                           small
                           :color="props.item.is_disabled ? 'error': 'success'">{{ props.item.is_disabled_str }}</v-btn>
                  </td>
                  <td class="text-xs-left">
                    <v-btn flat
                           small
                           href
                           color="primary"
                           @click="edit(props.item)">编辑</v-btn>
                    <v-btn flat
                           small
                           color="primary"
                           @click="disable(props.item)">{{props.item.is_disabled ? "启用" : "禁用"}}</v-btn>
                    <v-btn flat
                           small
                           color="primary"
                           @click="del(props.item)">删除</v-btn>
                  </td>
                </template>
                <template v-slot:footer>
                  <td :colspan="table.headers.length">
                    <div class="text-xs-center pt-2">
                      <v-pagination v-model="table.pageIndex"
                                    :length="table.pageCount"
                                    total-visible="10"></v-pagination>
                    </div>
                  </td>
                </template>
              </v-data-table>
            </v-card-text>
          </v-card>
        </v-flex>
      </v-layout>
    </v-container>
  </div>
</template>

<script>
import request from "@/util/request"
export default {
  $_veeValidate: {
    validator: 'new'
  },
  data () {
    return {
      search: "",
      dialog: false,
      formTitle: '',
      formItem: {},
      table: {
        pageIndex: 1,
        pageSize: 10,
        pageSizes: [10, 20, 50, 100, { text: '全部', value: -1 }],
        pageCount: 0,
        totalCount: 0,
        selected: [],
        headers: [
          {
            text: "用户名",
            value: "user_name",
            align: 'left',
            width: 150,
            sortable: false
          },
          {
            text: "微信",
            value: "wechat",
            align: 'left',
            width: 150,
            sortable: false
          },
          {
            text: "手机号",
            value: "tel",
            align: 'left',
            width: 180,
            sortable: false
          },
          {
            text: "状态",
            value: "is_disabled_str",
            align: 'left',
            width: 150,
            sortable: false
          },
          {
            text: "操作",
            value: "",
            align: 'left',
            sortable: false
          }
        ],
        items: []
      },
      dictionary: {
        custom: {
          user_name: {
            required: () => '用户名不能为空',
            max: '用户名长度不能超过15位'
            // custom messages
          },
          select: {
            required: 'Select field is required'
          }
        }
      }
    }
  },
  watch: {
    'table.pageIndex' (newVal, oldVal) {
      this.getList()
    }
  },
  mounted () {
    this.$validator.localize('zh', this.dictionary)
    this.getList()
  },
  methods: {
    //新增
    add () {
      this.getOne(0)
    },
    //修改
    edit (item) {
      this.getOne(item.id)
    },
    //禁用/启用
    disable (item) {
      this.$dialog.warning({
        text: `确定${item.is_disabled ? '启用' : '禁用'}用户"${item.user_name}"吗`,
        title: '提示',
        persistent: true,
        actions: {
          true: {
            color: 'primary',
            text: '确定',
            handle: () => {
              request({
                url: '/Api/User/Disable',
                method: 'post',
                data: item
              }).then(({ data }) => {
                if (data.Result) {
                  this.getList()
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
    //禁用/启用
    del (item) {
      this.$dialog.error({
        text: `确定删除用户"${item.user_name}"吗`,
        title: '警告',
        persistent: true,
        actions: {
          true: {
            color: 'primary',
            text: '确定',
            handle: () => {
              request({
                url: '/Api/User/Delete',
                method: 'post',
                data: item
              }).then(({ data }) => {
                if (data.Result) {
                  this.getList()
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
    //关闭窗口
    close () {
      this.dialog = false
      setTimeout(() => {
        this.formItem = {}
      }, 300)
    },
    //保存
    save () {
      var valid = this.$validator.validateAll()
      if (!valid) {
        return
      }
      request({
        url: '/Api/User/Add',
        method: 'post',
        data: this.formItem
      }).then(({ data }) => {
        if (data.Result) {
          this.getList()
          this.close()
          this.$dialog.message.success(data.Message, {
            position: 'top'
          })
        } else {
          this.$dialog.message.error(data.Message, {
            position: 'top'
          })
        }
      })
    },
    //获取单个
    getOne (id) {
      request({
        url: '/Api/User/GetOne',
        method: 'post',
        data: {
          id: id
        }
      }).then(({ data }) => {
        if (data.Result) {
          if (!this.dialog) {
            this.dialog = true
          }
          this.formItem = data.Data
          this.formTitle = id == 0 ? '新建用户' : '编辑用户'
        } else {
          this.$dialog.message.error(data.Message, {
            position: 'top'
          })
        }
      })
    },
    //获取列表
    getList () {
      request({
        url: '/Api/User/GetList',
        method: 'post',
        data: {
          page_index: this.table.pageIndex,
          page_size: this.table.pageSize
        }
      }).then(({ data }) => {
        if (data.Result) {
          this.table.items = data.Data
          this.table.pageCount = data.PageInfo.PageCount
          this.table.totalCount = data.PageInfo.TotalCount
        } else {
          this.$dialog.message.error(data.Message, {
            position: 'top'
          })
        }
      })
    }
  }
}
</script>
