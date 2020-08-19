var $url = '/gather/testingContent';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  ruleId: utils.getQueryInt('ruleId'),
  rule: null,
  contentUrl: decodeURIComponent(utils.getQueryString('contentUrl')),
  attributes: null
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId,
        ruleId: this.ruleId,
        contentUrl: this.contentUrl
      }
    }).then(function (response) {
      var res = response.data;

      $this.attributes = res.attributes;

      var head = document.head || document.getElementsByTagName('head')[0];
      var base = document.createElement('base');
      base.setAttribute("href", $this.contentUrl);
      head.appendChild(base); 
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
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
