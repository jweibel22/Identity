angular.module('inspire').factory('feedService', ['$http', function($http){

    var o = {
        posts: [],
        timestamp: new Date(),
        formattedTimestamp: function() {
            var s = o.timestamp.getFullYear() + "-" + (o.timestamp.getMonth()+1) + "-" + o.timestamp.getDate() + " " + o.timestamp.getHours() + ":"
                + o.timestamp.getMinutes() + ":" + o.timestamp.getSeconds() + "." + o.timestamp.getMilliseconds();

            return s;
        }
    };

    o.getFeed = function (orderBy) {
        return $http.get('/Api/Feed?fromIndex=0&timestamp=' + o.formattedTimestamp() +"&orderBy="+orderBy).success(function (data) {
            angular.copy(data, o.posts);
        });
    };

    o.loadMorePosts = function (orderBy) {
        return $http.get('/Api/Feed?fromIndex=' + o.posts.length + "&timestamp=" + o.formattedTimestamp() + "&orderBy=" + orderBy).success(function (data) {
            angular.copy(o.posts.concat(data), o.posts);
        });
    };

    return o;
}]);
