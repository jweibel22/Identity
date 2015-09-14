angular.module('inspire')
    .controller('SearchController', [
        '$scope', '$http', '$stateParams', 'postService', 'feedService','tagService', 'userPromise',
        function($scope, $http, $stateParams, postService, feedService, tagService, userPromise ){

            $scope.query = $stateParams.query;
            $scope.user = userPromise.data;
            $scope.posts = postService.queryresults;

            //$scope.savePost = function(post) {
            //    postService.savePost($scope.user.DefaultChannel, post)
            //        .success(function(post1) {
            //        });
            //}

            //$scope.follows = function(tag) {
            //    return $scope.user.FollowsTags.indexOf(tag) > -1;
            //}

            //$scope.follow = function(tag) {
            //    tagService.follow(tag).success(function(data) {
            //        $scope.user.FollowsTags.push(tag);
            //    });
            //}

            //$scope.unfollow = function(tag) {
            //    tagService.unfollow(tag).success(function(data) {
            //        $scope.user.FollowsTags.splice($scope.user.FollowsTags.indexOf(tag), 1);
            //    });
            //}
        }]);