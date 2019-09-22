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
                         @click="add">新建{{basic.title}}</v-btn>
                </template>
                <v-card>
                  <v-card-title>
                    <span class="headline">{{ formTitle }}</span>
                  </v-card-title>

                  <v-card-text>
                    <v-container grid-list-md>
                      <form>
                        <template v-for="(item,index) in columns">
                          <v-flex v-if="item.form"
                                  :key="index">
                            <v-text-field clearable
                                          v-model="formItem[item.value]"
                                          v-validate="item.validate"
                                          :counter="item.counter"
                                          :error-messages="errors.collect(item.value)"
                                          :data-vv-name="item.value"
                                          :required="item.required"
                                          :label="item.text"></v-text-field>
                          </v-flex>
                        </template>
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
              <v-data-table :headers="columns"
                            :items="table.items"
                            :rows-per-page-items="table.pageSizes"
                            class="elevation-1"
                            item-key="name"
                            hide-actions>
                <template slot="items"
                          slot-scope="props">
                  <template v-for="(item,index) in columns">
                    <template v-if="item.type != 'action'">
                      <td :key="index"
                          class="text-xs-left">{{ props.item[item.value] }}</td>
                    </template>
                    <template v-else>
                      <td :key="index"
                          class="text-xs-left">
                        <v-btn v-for="(action,bIndex) in item.actions"
                               :key="bIndex"
                               flat
                               small
                               href
                               color="primary"
                               @click="action.handle(props.item)">{{action.name}}</v-btn>
                      </td>
                    </template>
                  </template>
                </template>
                <template v-slot:footer>
                  <td :colspan="columns.length">
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
  props: {
    basic: {
      type: Object
    },
    curd: {
      type: Object
    },
    columns: {
      type: Array,
      default: []
    }
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
        items: []
      }
    }
  },
  watch: {
    'table.pageIndex' (newVal, oldVal) {
      this.getList()
    }
  },
  mounted () {
    var dictionary = { custom: {} }
    for (let col of this.columns) {
      if (col.required && col.type != 'action') {
        dictionary.custom[col.value] = col.requiredInfo
      }
    }
    this.$validator.localize('zh', dictionary)
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
        text: `确定删除${this.basic.title}"${item[this.showValue]}"吗`,
        title: '警告',
        persistent: true,
        actions: {
          true: {
            color: 'primary',
            text: '确定',
            handle: () => {
              request({
                url: `/Api/${this.basic.controller}/Delete`,
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
            url: `/Api/${this.basic.controller}/Add`,
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
        url: `/Api/${this.basic.controller}/GetOne`,
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
          this.formTitle = id == 0 ? `新建${this.basic.title}` : `编辑${this.basic.title}`
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
        url: `/Api/${this.basic.controller}/GetList`,
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
