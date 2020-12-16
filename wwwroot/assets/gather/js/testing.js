var $url = '/gather/testing';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  ruleId: utils.getQueryInt('ruleId'),
  rule: null,
  gatherUrls: null,
  listUrl: null,
  contentUrls: null,
  imageUrls: null
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

      $this.contentUrls = res.contentUrls;
      $this.imageUrls = res.imageUrls;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  getImageUrl: function(contentUrl) {
    var index = this.contentUrls.indexOf(contentUrl);
    if (index !== -1 && this.imageUrls.length > index) {
      return this.imageUrls[index];
    }
    return '';
  },

  btnGetContentUrls: function () {
    this.apiSubmit();
  },

  btnGetContent: function(contentUrl) {
    utils.addTab('测试获取内容', utils.getPageUrl('gather', 'testingContent', {
      siteId: this.siteId,
      ruleId: this.ruleId,
      contentUrl: encodeURIComponent(contentUrl)
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
