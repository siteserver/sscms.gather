var $url = '/gather/list';
var $urlExport = $url + '/actions/export';
var $urlImport = $url + '/actions/import';
var $urlDelete = $url + '/actions/delete';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  rules: null,
  urlUpload: null,
  multipleSelection: [],
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: $this.siteId
      }
    }).then(function (response) {
      var res = response.data;

      $this.rules = res.rules;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiDelete: function (ruleIds) {
    var $this = this;

    utils.loading(this, true);
    $api.post($urlDelete, {
      siteId: $this.siteId,
      ruleIds: ruleIds
    }).then(function (response) {
      var res = response.data;

      $this.rules = res.rules;
      utils.success('采集规则删除成功');
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnAddClick: function () {
    location.href = utils.getPageUrl('gather', 'add', {
      siteId: this.siteId
    });
  },

  btnTestClick: function (rule) {
    utils.addTab('测试采集规则：' + rule.ruleName, utils.getPageUrl('gather', 'testing', {
      siteId: this.siteId,
      ruleId: rule.id
    }));
  },

  btnStartSelectedClick: function () {
    utils.addTab('批量采集', utils.getPageUrl('gather', 'status', {
      siteId: this.siteId,
      ruleIds: this.dataIds.join(',')
    }));
  },

  btnStartClick: function (rule) {
    utils.addTab('开始采集：' + rule.ruleName, utils.getPageUrl('gather', 'start', {
      siteId: this.siteId,
      ruleId: rule.id
    }));
  },

  btnSingleClick: function (rule) {
    utils.addTab('单页采集：' + rule.ruleName, utils.getPageUrl('gather', 'single', {
      siteId: this.siteId,
      ruleId: rule.id
    }));
  },

  btnCopyClick: function(rule) {
    utils.openLayer({
      title: '复制采集规则',
      url: utils.getPageUrl('gather', 'layerCopy', {
        siteId: this.siteId,
        ruleId: rule.id
      }),
      width: 400,
      height: 200
    })
  },

  btnEditClick: function (rule) {
    utils.addTab('编辑：' + rule.ruleName, utils.getPageUrl('gather', 'add', {
      siteId: this.siteId,
      ruleId: rule.id
    }));
  },

  btnExportClick: function (rule) {
    window.open($apiUrl + $urlExport + '?siteId=' + this.siteId + '&ruleId=' + rule.id + '&access_token=' + $token);
  },

  btnDeleteSelectedClick: function () {
    var $this = this;

    utils.alertDelete({
      title: '删除所选数据',
      text: '此操作将删除所选数据，确定吗？',
      callback: function () {
        $this.apiDelete($this.dataIds);
      }
    });
  },

  btnDeleteClick: function (rule) {
    var $this = this;

    utils.alertDelete({
      title: '删除采集规则',
      text: '此操作将删除采集规则' + rule.ruleName + '，确定吗？',
      callback: function () {
        $this.apiDelete([rule.id]);
      }
    });
  },

  toggleSelection: function(row) {
    this.$refs.multipleTable.toggleRowSelection(row);
  },

  handleSelectionChange: function(val) {
    this.multipleSelection = val;
  },

  tableRowClassName: function(scope) {
    if (this.multipleSelection.indexOf(scope.row) !== -1) {
      return 'current-row';
    }
    return '';
  },

  uploadBefore(file) {
    var re = /(\.json)$/i;
    if(!re.exec(file.name))
    {
      utils.error('上传格式错误，请上传json压缩包!');
      return false;
    }

    return true;
  },

  uploadProgress: function() {
    utils.loading(this, true);
  },

  uploadSuccess: function(res, file) {
    utils.loading(this, false);

    utils.success('采集规则导入成功');
    location.reload();
  },

  uploadError: function(err) {
    utils.loading(this, false);
    var error = JSON.parse(err.message);
    utils.error(error.message);
  }
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  computed: {
    isChecked: function() {
      return this.multipleSelection.length > 0;
    },

    dataIds: function() {
      var retVal = [];
      for (var i = 0; i < this.multipleSelection.length; i++) {
        var item = this.multipleSelection[i];
        retVal.push(item.id);
      }
      return retVal;
    },
  },
  created: function () {
    this.apiGet();
    this.urlUpload = $apiUrl + $urlImport + '?siteId=' + this.siteId;
  }
});
