angular.module('inspire').factory('userService', ['$http', function($http){

    var o = {

    };

    o.getCurrentUser = function() {
        return $http.get('/Api/User');
    };


    return o;
}]);
