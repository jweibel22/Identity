angular.module('inspire').factory('feedService', ['$http', function($http){

    var o = {
        posts: []
    };

    o.getFeed = function() {
        return $http.get('/Api/Feed').success(function(data){
            angular.copy(data, o.Posts);
        });
    };

    return o;
}]);
