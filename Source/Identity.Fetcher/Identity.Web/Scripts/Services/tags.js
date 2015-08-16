angular.module('inspire').factory('tagService', ['$http', 'ngSettings', function ($http, ngSettings) {

    var o = {

    };

    o.follow = function(tag) {
        return $http.put(ngSettings.baseUrl + '/tags/' + tag + "/follow");
    };

    o.unfollow = function(tag) {
        return $http.put(ngSettings.baseUrl + '/tags/' + tag + "/unfollow");
    };

    return o;
}]);
