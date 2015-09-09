angular.module('inspire').factory('userService', ['$http', 'ngSettings', function ($http, ngSettings) {

    var o = {
        searchResult: []
    };

    o.getCurrentUser = function() {
        return $http.get(ngSettings.baseUrl + '/Api/User');
    };

    o.getUser = function (userId) {
        return $http.get(ngSettings.baseUrl + '/Api/User/' + userId);
    };

    o.findByName = function (query) {
        return $http.get(ngSettings.baseUrl + '/Api/User?query=' + query).success(function (data) {
            angular.copy(data, o.searchResult);
        });
    }

    return o;
}]);
