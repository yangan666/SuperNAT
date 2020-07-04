<template>
  <div class="list-table">
    <v-container grid-list-xl fluid>
      <v-layout row wrap>
        <v-flex lg12>
          <v-card>
            <!-- 菜单栏 -->
            <v-toolbar card color="white">
              <v-text-field
                flat
                solo
                clearable
                prepend-icon="search"
                @click:prepend="
                  table.pageIndex = 1
                  getList()
                "
                placeholder="请输入关键词"
                v-model="search"
                hide-details
                class="hidden-sm-and-down"
              ></v-text-field>
              <!-- 弹框 -->
              <v-dialog v-model="dialog" persistent :width="basic.dialogWith || 500">
                <template v-slot:activator="{ on }">
                  <v-btn
                    color="primary"
                    dark
                    class="mb-2"
                    v-on="on"
                    @click="add"
                  >新建{{ basic.title }}</v-btn>
                </template>
                <v-card>
                  <v-card-title>
                    <span class="headline">{{ formTitle }}</span>
                  </v-card-title>

                  <v-card-text>
                    <v-container grid-list-md>
                      <!-- 动态form表单 -->
                      <form>
                        <v-layout row wrap>
                          <template v-for="(item, index) in forms">
                            <!-- 输入框 -->
                            <v-flex
                              v-if="item.type == 'input'"
                              v-bind="{ [`xs${item.formRowXs || 12}`]: true }"
                              :key="index"
                            >
                              <v-text-field
                                clearable
                                v-model="formItem[item.value]"
                                v-validate="item.validate"
                                :counter="item.counter"
                                :error-messages="errors.collect(item.value)"
                                :data-vv-name="item.value"
                                :data-vv-as="item.text"
                                :label="item.text"
                              ></v-text-field>
                            </v-flex>
                            <!-- 密码框 -->
                            <v-flex
                              v-else-if="item.type == 'password'"
                              v-bind="{ [`xs${item.formRowXs || 12}`]: true }"
                              :key="index"
                            >
                              <v-text-field
                                clearable
                                type="password"
                                v-model="formItem[item.value]"
                                v-validate="item.validate"
                                :counter="item.counter"
                                :error-messages="errors.collect(item.value)"
                                :data-vv-name="item.value"
                                :data-vv-as="item.text"
                                :label="item.text"
                              ></v-text-field>
                            </v-flex>
                            <!-- 下拉框 -->
                            <v-flex
                              v-else-if="item.type == 'select'"
                              v-bind="{ [`xs${item.formRowXs || 12}`]: true }"
                              :key="index"
                            >
                              <v-select
                                clearable
                                v-model="formItem[item.value]"
                                v-validate="item.validate"
                                :error-messages="errors.collect(item.value)"
                                :data-vv-name="item.value"
                                :data-vv-as="item.text"
                                :items="item.items"
                                :item-text="item.itemText"
                                :item-value="item.itemValue"
                                @change="item.change"
                                :label="item.text"
                              ></v-select>
                            </v-flex>
                            <!-- 开关 -->
                            <v-flex
                              v-else-if="item.type == 'switch'"
                              v-bind="{ [`xs${item.formRowXs || 12}`]: true }"
                              :key="index"
                            >
                              <v-switch
                                clearable
                                v-model="formItem[item.value]"
                                v-validate="item.validate"
                                :error-messages="errors.collect(item.value)"
                                :data-vv-name="item.value"
                                :data-vv-as="item.text"
                                @change="item.change"
                                :label="item.text"
                              ></v-switch>
                            </v-flex>
                            <!-- 提示框 -->
                            <v-flex
                              v-else-if="item.type == 'alert'"
                              v-bind="{ [`xs${item.formRowXs || 12}`]: true }"
                              :key="index"
                            >
                              <v-alert :value="true" type="info">{{formItem[item.value]}}</v-alert>
                            </v-flex>
                          </template>
                        </v-layout>
                      </form>
                    </v-container>
                  </v-card-text>

                  <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="blue darken-1" flat @click="save">保存</v-btn>
                    <v-btn color="blue darken-1" flat @click="close">取消</v-btn>
                  </v-card-actions>
                </v-card>
              </v-dialog>
            </v-toolbar>
            <v-divider></v-divider>
            <!-- 查询记录数信息 -->
            <v-alert :value="true" type="info">查询结果共{{table.totalCount}}条记录</v-alert>
            <v-divider></v-divider>
            <!-- 表格数据 -->
            <v-card-text class="pa-0">
              <v-data-table
                :headers="headers"
                :items="table.items"
                :rows-per-page-items="table.pageSizes"
                class="elevation-1"
                item-key="name"
                disable-initial-sort
                hide-actions
              >
                <template slot="items" slot-scope="props">
                  <template v-for="(item, index) in headers">
                    <td v-if="item.type == 'action'" :key="index" class="text-xs-left">
                      <v-btn
                        v-for="(action, bIndex) in item.actions"
                        :key="bIndex"
                        flat
                        small
                        href
                        color="primary"
                        @click="action.handle(props.item)"
                      >{{ action.name(props.item) }}</v-btn>
                    </td>
                    <td v-else-if="item.type == 'tag'" :key="index" class="text-xs-left">
                      <v-btn flat small :color="item.color(props.item)">{{ props.item[item.value] }}</v-btn>
                    </td>
                    <td
                      v-else
                      :key="index"
                      class="text-xs-left"
                    >{{ item.textFormat ? item.textFormat(props.item) : props.item[item.value] }}</td>
                  </template>
                </template>
                <!-- 分页 -->
                <template v-slot:footer>
                  <td :colspan="headers.length">
                    <div class="text-xs-center pt-2">
                      <v-pagination
                        v-model="table.pageIndex"
                        :length="table.pageCount"
                        total-visible="10"
                      ></v-pagination>
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
    validator: "new"
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
  data() {
    return {
      search: "",
      dialog: false,
      formTitle: "",
      formItem: {},
      table: {
        pageIndex: 1,
        pageSize: 10,
        pageSizes: [10, 20, 50, 100, { text: "全部", value: -1 }],
        pageCount: 0,
        totalCount: 0,
        selected: [],
        items: []
      }
    }
  },
  computed: {
    headers() {
      return this.columns.filter(c => c.table)
    },
    forms() {
      return this.columns.filter(c => c.form)
    }
  },
  watch: {
    "table.pageIndex"(newVal, oldVal) {
      this.getList()
    }
  },
  mounted() {
    var dictionary = { custom: {} }
    for (let col of this.columns) {
      if (col.requiredInfo) {
        dictionary.custom[col.value] = col.requiredInfo
      }
    }
    this.$validator.localize("zh_CN", dictionary)
    this.getList()
  },
  methods: {
    //新增
    add() {
      this.getOne(0)
      this.curd.affterAdd()
    },
    //修改
    edit(item) {
      this.getOne(item.id)
    },
    //删除
    del(item) {
      this.$dialog.error({
        text: `确定删除${this.basic.title}"${item[this.basic.showValue]}"吗`,
        title: "警告",
        persistent: true,
        actions: {
          true: {
            color: "primary",
            text: "确定",
            handle: () => {
              request({
                url: `/Api/${this.basic.controller}/Delete`,
                method: "post",
                data: item
              }).then(({ data }) => {
                if (data.Result) {
                  this.getList()
                  this.$dialog.message.success(data.Message, {
                    position: "top"
                  })
                } else {
                  this.$dialog.message.error(data.Message, {
                    position: "top"
                  })
                }
              })
            }
          },
          false: "取消"
        }
      })
    },
    //关闭窗口
    close() {
      this.dialog = false
      setTimeout(() => {
        this.formItem = {}
        this.$validator.reset()
      }, 300)
    },
    //保存
    save() {
      this.$validator.validateAll().then(res => {
        if (res) {
          request({
            url: `/Api/${this.basic.controller}/Add`,
            method: "post",
            data: this.formItem
          }).then(({ data }) => {
            if (data.Result) {
              this.getList()
              this.close()
              this.$dialog.message.success(data.Message, {
                position: "top"
              })
            } else {
              this.$dialog.message.error(data.Message, {
                position: "top"
              })
            }
          })
        }
      })
    },
    //获取单个
    getOne(id) {
      request({
        url: `/Api/${this.basic.controller}/GetOne`,
        method: "post",
        data: {
          id: id
        }
      }).then(({ data }) => {
        if (data.Result) {
          if (!this.dialog) {
            this.dialog = true
          }
          this.curd.affterGetOne(data.Data)
          this.formItem = data.Data
          this.formTitle = id == 0 ? `新建${this.basic.title}` : `编辑${this.basic.title}`
        } else {
          this.$dialog.message.error(data.Message, {
            position: "top"
          })
        }
      })
    },
    //获取列表
    getList() {
      request({
        url: `/Api/${this.basic.controller}/GetList`,
        method: "post",
        data: {
          search: this.search,
          user_id: this.$store.getters.user.user_id,
          is_admin: this.$store.getters.user.is_admin,
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
            position: "top"
          })
        }
      })
    }
  }
}
</script>
