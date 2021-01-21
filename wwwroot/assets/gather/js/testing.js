var $url = '/gather/testing';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  ruleId: utils.getQueryInt('ruleId'),
  rule: null,
  gatherUrls: null,
  listUrl: null,
  items: null
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId,
        ruleId: this.ruleId
      }
    }).then(function (response) {
      var res = response.data;

      $this.rule = res.rule;
      $this.gatherUrls = res.gatherUrls;
      $this.listUrl = res.gatherUrls[0];
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSubmit: function () {
    var $this = this;

    utils.loading(this, true);
    $api.post($url, {
      siteId: this.siteId,
      ruleId: this.ruleId,
      gatherUrl: this.listUrl
    }).then(function (response) {
      var res = response.data;

      $this.items = res.items;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  getImageUrl: function(item) {
    return item.content.imageUrl;
  },

  btnGetContentUrls: function () {
    this.apiSubmit();
  },

  btnGetClick: function(item) {
    utils.addTab('测试获取内容', utils.getPageUrl('gather', 'testingContent', {
      siteId: this.siteId,
      ruleId: this.ruleId,
      listUrl: encodeURIComponent(this.listUrl),
      contentUrl: encodeURIComponent(item.url)
    }));
  },

  btnCloseClick: function () {
    utils.removeTab();
  }
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  created: function () {
    this.apiGet();
  }
});
