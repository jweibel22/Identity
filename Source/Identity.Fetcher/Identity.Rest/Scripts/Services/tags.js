angular.module('inspire').factory('tagService', ['$http', function($http){

    var o = {

    };

    o.follow = function(tag) {
        return $http.put('/tags/' + tag + "/follow");
    };

    o.unfollow = function(tag) {
        return $http.put('/tags/' + tag + "/unfollow");
    };

    return o;
}]);
