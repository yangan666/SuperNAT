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
                            clearable
                            prepend-icon="search"
                            @click:prepend='table.pageIndex=1;getList()'
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
                         @click="add">新建角色</v-btn>
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
                                        v-model="formItem.name"
                                        v-validate="'required'"
                                        :error-messages="errors.collect('name')"
                                        data-vv-name="name"
                                        required
                                        label="角色名称"></v-text-field>
                        </v-flex>
                        <v-flex>
                          <v-text-field clearable
                                        v-model="formItem.remark"
                                        label="描述"></v-text-field>
                        </v-flex>
                        <v-flex>

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
                            :items="table.items"
                            :rows-per-page-items="table.pageSizes"
                            class="elevation-1"
                            item-key="name"
                            hide-actions>
                <template slot="items"
                          slot-scope="props">
                  <td class="text-xs-left">{{ props.item.name }}</td>
                  <td class="text-xs-left">{{ props.item.remark }}</td>
                  <td class="text-xs-left">
                    <v-btn flat
                           small
                           href
                           color="primary"
                           @click="edit(props.item)">编辑</v-btn>
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
            text: "角色名",
            align: 'left',
            width: 150,
            sortable: false
          },
          {
            text: "描述",
            align: 'left',
            width: 300,
            sortable: false
          },
          {
            text: "操作",
            align: 'left',
            sortable: false
          }
        ],
        items: []
      },
      dictionary: {
        custom: {
          name: {
            required: () => '角色名不能为空'
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
    //删除
    del (item) {
      this.$dialog.error({
        text: `确定删除角色"${item.name}"吗`,
        title: '警告',
        persistent: true,
        actions: {
          true: {
            color: 'primary',
            text: '确定',
            handle: () => {
              request({
                url: '/Api/Role/Delete',
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
        this.$validator.reset()
      }, 300)
    },
    //保存
    save () {
      this.$validator.validateAll(this.formItem).then(res => {
        if (res) {
          request({
            url: '/Api/Role/Add',
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
        }
      })
    },
    //获取单个
    getOne (id) {
      request({
        url: '/Api/Role/GetOne',
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
          this.formTitle = id == 0 ? '新建角色' : '编辑角色'
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
        url: '/Api/Role/GetList',
        method: 'post',
        data: {
          user_name: this.search,
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