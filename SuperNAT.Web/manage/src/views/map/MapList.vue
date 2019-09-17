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
                         @click="add">新建主机</v-btn>
                </template>
                <v-card>
                  <v-card-title>
                    <span class="headline">{{ formTitle }}</span>
                  </v-card-title>

                  <v-card-text>
                    <v-container grid-list-md>
                      <form>
                        <v-flex>
                          <v-select clearable
                                    v-model="formItem.user_id"
                                    v-validate="'required'"
                                    :error-messages="errors.collect('user_id')"
                                    data-vv-name="user_id"
                                    required
                                    :items="userList"
                                    item-text="user_name"
                                    item-value="user_id"
                                    @change='selectUserChange'
                                    label="所属用户"></v-select>
                        </v-flex>
                        <v-flex>
                          <v-select clearable
                                    v-model="formItem.client_id"
                                    v-validate="'required'"
                                    :error-messages="errors.collect('client_id')"
                                    data-vv-name="client_id"
                                    required
                                    :items="clientOptions"
                                    item-text="name"
                                    item-value="id"
                                    label="选择主机"></v-select>
                        </v-flex>
                        <v-flex>
                          <v-text-field clearable
                                        v-model="formItem.name"
                                        v-validate="'required'"
                                        :error-messages="errors.collect('name')"
                                        data-vv-name="name"
                                        required
                                        label="应用名称"></v-text-field>
                        </v-flex>
                        <v-flex>
                          <v-text-field clearable
                                        v-model="formItem.local"
                                        v-validate="'required'"
                                        :error-messages="errors.collect('local')"
                                        data-vv-name="local"
                                        required
                                        label="内网地址"></v-text-field>
                        </v-flex>
                        <v-flex>
                          <v-text-field clearable
                                        v-model="formItem.remote"
                                        v-validate="'required'"
                                        :error-messages="errors.collect('remote')"
                                        data-vv-name="remote"
                                        required
                                        label="外网地址"></v-text-field>
                        </v-flex>
                        <v-flex>
                          <v-select clearable
                                    v-model="formItem.protocol"
                                    v-validate="'required'"
                                    :error-messages="errors.collect('protocol')"
                                    data-vv-name="protocol"
                                    required
                                    :items="protocolOptions"
                                    label="协议类型"></v-select>
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
                  <td class="text-xs-left">{{ props.item.user_name }}</td>
                  <td class="text-xs-left">{{ props.item.client_name }}</td>
                  <td class="text-xs-left">{{ props.item.name }}</td>
                  <td class="text-xs-left">{{ props.item.local }}</td>
                  <td class="text-xs-left">{{ props.item.remote }}</td>
                  <td class="text-xs-left">{{ props.item.protocol }}</td>
                  <td class="text-xs-left">{{ props.item.certfile }}</td>
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
      userList: [],
      clientList: [],
      clientOptions: [],
      protocolOptions: ['http', 'https'],
      table: {
        pageIndex: 1,
        pageSize: 10,
        pageSizes: [10, 20, 50, 100, { text: '全部', value: -1 }],
        pageCount: 0,
        totalCount: 0,
        selected: [],
        headers: [
          {
            text: "所属用户",
            align: 'left',
            width: 100,
            sortable: false
          },
          {
            text: "主机名称",
            align: 'left',
            width: 150,
            sortable: false
          },
          {
            text: "应用名称",
            align: 'left',
            width: 150,
            sortable: false
          },
          {
            text: "内网地址",
            align: 'left',
            width: 150,
            sortable: false
          },
          {
            text: "外网地址",
            align: 'left',
            width: 150,
            sortable: false
          },
          {
            text: "协议类型",
            align: 'left',
            width: 180,
            sortable: false
          },
          {
            text: "证书文件",
            align: 'left',
            width: 250,
            sortable: false
          },
          {
            text: "操作",
            align: 'left',
            width: 200,
            sortable: false
          }
        ],
        items: []
      },
      dictionary: {
        custom: {
          user_id: {
            required: () => '请选择所属用户'
          },
          client_id: {
            required: () => '请选择主机'
          },
          name: {
            required: () => '应用名称不能为空'
          },
          local: {
            required: () => '内网地址不能为空'
          },
          remote: {
            required: () => '外网地址不能为空'
          },
          protocol: {
            required: () => '请选择协议类型'
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
      this.getUserList()
      this.getClientList()
    },
    //修改
    edit (item) {
      this.getOne(item.id)
      this.getUserList()
      this.getClientList()
    },
    //删除
    del (item) {
      this.$dialog.error({
        text: `确定删除应用"${item.name}"吗`,
        title: '警告',
        persistent: true,
        actions: {
          true: {
            color: 'primary',
            text: '确定',
            handle: () => {
              request({
                url: '/Api/Map/Delete',
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
            url: '/Api/Map/Add',
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
        url: '/Api/Map/GetOne',
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
          this.formTitle = id == 0 ? '新建主机' : '编辑主机'
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
        url: '/Api/Map/GetList',
        method: 'post',
        data: {
          name: this.search,
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
    },
    //用户列表
    getUserList () {
      request({
        url: '/Api/User/GetList',
        method: 'post',
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.userList = data.Data
        }
      })
    },
    //主机列表
    getClientList () {
      request({
        url: '/Api/Client/GetList',
        method: 'post',
        data: {}
      }).then(({ data }) => {
        if (data.Result) {
          this.clientList = data.Data
        }
      })
    },
    //选择所属用户事件
    selectUserChange (id) {
      if (id) {
        this.clientOptions = this.clientList.filter(c => c.user_id == id)
      } else {
        this.formItem.client_id = 0
        this.clientOptions = []
      }
    }
  }
}
</script>
