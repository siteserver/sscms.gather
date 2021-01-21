var $url = '/gather/list';
var $urlExport = '/gather/list/actions/export';
var $urlImport = '/gather/list/actions/import';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  rules: null,
  urlUpload: null
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

  apiDelete: function (rule) {
    var $this = this;

    utils.loading(this, true);
    $api.delete($url, {
      data: {
        siteId: $this.siteId,
        ruleId: rule.id
      }
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

  btnTestClick: function (rule) {
    utils.addTab('测试采集规则：' + rule.ruleName, utils.getPageUrl('gather', 'testing', {
      siteId: this.siteId,
      ruleId: rule.id
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

  btnDeleteClick: function (rule) {
    var $this = this;

    utils.alertDelete({
      title: '删除采集规则',
      text: '此操作将删除采集规则' + rule.ruleName + '，确定吗？',
      callback: function () {
        $this.apiDelete(rule);
      }
    });
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
  created: function () {
    this.apiGet();
    this.urlUpload = $apiUrl + $urlImport + '?siteId=' + this.siteId;
  }
});
