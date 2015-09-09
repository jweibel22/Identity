angular.module('inspire').factory('postService', ['$http', '$q', 'ngSettings', function ($http, $q, ngSettings) {
    var o = {
        posts: [],
        queryresults: [],
        timestamp: new Date(),
        formattedTimestamp: function () {
            var s = o.timestamp.getFullYear() + "-" + (o.timestamp.getMonth() + 1) + "-" + o.timestamp.getDate() + " " + o.timestamp.getHours() + ":"
                + o.timestamp.getMinutes() + ":" + o.timestamp.getSeconds() + "." + o.timestamp.getMilliseconds();

            return s;
        }
    };

    o.getByTag = function(tag) {
        var promise = $http.get(ngSettings.baseUrl + '/Api/Post?tag=' + tag);

        promise.success(function(data){
            angular.copy(data, o.queryresults);
        });
        return promise;
    };

    o.getFromChannel = function (channelId, onlyUnread, orderBy) {
        var fromIndex = 0;
        var promise = $http.get(ngSettings.baseUrl + '/Api/Channel/' + channelId + "?onlyUnread=" + onlyUnread + "&timestamp=" + o.formattedTimestamp() + "&fromIndex=" + fromIndex + "&orderBy=" + orderBy);

        promise.success(function (data) {
            if (!o.posts[channelId]) {
                o.posts[channelId] = [];
            }
            angular.copy(data.Posts, o.posts[channelId]);
        });
        return promise;
    };

    o.loadMorePosts = function (channelId, onlyUnread, orderBy) {
        var fromIndex = (o.posts[channelId] ? o.posts[channelId].length : 0);
        var promise = $http.get(ngSettings.baseUrl + '/Api/Channel/' + channelId + "?onlyUnread=" + onlyUnread + "&timestamp=" + o.formattedTimestamp() + "&fromIndex=" + fromIndex + "&orderBy=" + orderBy);

            promise.success(function(data){
            if (!o.posts[channelId]) {
                o.posts[channelId] = [];
            }
            angular.copy(o.posts[channelId].concat(data.Posts), o.posts[channelId]);
        });
        return promise;
    };

    o.getFromDefaultChannel = function() {

        var deferred = $q.defer();

        $http.get(ngSettings.baseUrl + '/Api/User').success(function (user) {

            var fromIndex = (o.posts[user.SavedChannel] ? o.posts[user.SavedChannel].length : 0);
            $http.get(ngSettings.baseUrl + '/Api/Channel/' + user.SavedChannel + "?onlyUnread=false" + "&timestamp=" + o.formattedTimestamp() + "&fromIndex=" + fromIndex + "&orderBy=Added").success(function (data) {
                if (!o.posts[user.SavedChannel]) {
                    o.posts[user.SavedChannel] = [];
                }
                angular.copy(data.Posts, o.posts[user.SavedChannel]);
                deferred.resolve({data: data});
            });
        });

        return deferred.promise;
    };

    o.create = function(post, channelId) {
        return $http.post(ngSettings.baseUrl + '/Api/Channel/' + channelId + '/Posts', post).success(function (data) {
            o.posts[channelId].push(data);
        }).error(function(data) {
            console.log(data);
        });
    };

    o.savePost = function(channelId, post) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + '/Posts?postId=' + post.Id).success(function (data) {
            o.posts[channelId].push(post);
        });
    };

    o.upvote = function(post) {
        return $http.put(ngSettings.baseUrl + '/Api/Post/' + post.Id + '/Upvote')
            .success(function(data){
                post.upvotes += 1;
            });
    };

    o.get = function(id) {
        return $http.get(ngSettings.baseUrl + '/Api/Post/' + id).then(function (res) {
            return res.data;
        });
    };

    o.addComment = function(id, comment) {
        return $http.post(ngSettings.baseUrl + '/Api/Post/' + id + '/Comments', comment);
    };

    o.read = function (id, userId) {
        return $http.post(ngSettings.baseUrl + '/Api/Post/' + id + '/Read?userId=' + userId);
    };

    o.editPost = function(post) {
        return $http.post(ngSettings.baseUrl + '/Api/Post/' + post.Id, post);
    };

    o.deletePost = function(channelId, post) {
        return $http.delete(ngSettings.baseUrl + '/Api/Channel/' + channelId + '/DeletePost?postId=' + post.Id);
    };

    o.upvoteComment = function(post, comment) {
        return $http.put(ngSettings.baseUrl + '/posts/' + post._id + '/comments/' + comment._id + '/upvote')
            .success(function(data){
                comment.upvotes += 1;
            });
    };

    return o;
}]);