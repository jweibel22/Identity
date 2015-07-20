angular.module('inspire').factory('postService', ['$http', '$q', function($http, $q){
    var o = {
        posts: [],
        queryresults: []
    };

    o.getByTag = function(tag) {
        var promise = $http.get('/Api/Post?tag=' + tag);

        promise.success(function(data){
            angular.copy(data, o.queryresults);
        });
        return promise;
    };

    o.getFromChannel = function (channelId, onlyUnread) {
        var promise = $http.get('/Api/Channel/' + channelId + "?onlyUnread="+onlyUnread);

            promise.success(function(data){
            if (!o.posts[channelId]) {
                o.posts[channelId] = [];
            }
            angular.copy(data.Posts, o.posts[channelId]);
        });
        return promise;
    };

    o.getFromDefaultChannel = function() {

        var deferred = $q.defer();

        $http.get('/Api/User').success(function(user) {

            $http.get('/Api/Channel/' + user.SavedChannel + "?onlyUnread=false").success(function (data) {
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
        return $http.post('/Api/Channel/' + channelId + '/Posts', post).success(function(data) {
            o.posts[channelId].push(data);
        }).error(function(data) {
            console.log(data);
        });
    };

    o.savePost = function(channelId, post) {
        return $http.put('/Api/Channel/' + channelId + '/Posts?postId=' + post.Id).success(function(data){
            o.posts[channelId].push(post);
        });
    };

    o.upvote = function(post) {
        return $http.put('/Api/Post/' + post.Id + '/Upvote')
            .success(function(data){
                post.upvotes += 1;
            });
    };

    o.get = function(id) {
        return $http.get('/Api/Post/' + id).then(function(res){
            return res.data;
        });
    };

    o.addComment = function(id, comment) {
        return $http.post('/Api/Post/' + id + '/Comments', comment);
    };

    o.read = function (id, userId) {
        return $http.post('/Api/Post/' + id + '/Read?userId='+userId);
    };

    o.editPost = function(post) {
        return $http.post('/Api/Post/' + post.Id, post);
    };

    o.deletePost = function(channelId, post) {
        return $http.delete('/Api/Channel/' + channelId + '/DeletePost?postId=' + post.Id);
    };

    o.upvoteComment = function(post, comment) {
        return $http.put('/posts/' + post._id + '/comments/'+ comment._id + '/upvote')
            .success(function(data){
                comment.upvotes += 1;
            });
    };

    return o;
}]);