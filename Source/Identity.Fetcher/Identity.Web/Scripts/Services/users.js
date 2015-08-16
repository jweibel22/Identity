angular.module('inspire').factory('userService', ['$http', 'ngSettings', function ($http, ngSettings) {

    var o = {

    };

    o.getCurrentUser = function() {
        return $http.get(ngSettings.baseUrl + '/Api/User');
    };


    return o;
}]);
