angular.module('inspire')
    .controller('FeedController', [
        '$scope', '$http', '$stateParams', '$filter', 'postService', 'feedService','tagService', 'userPromise',
        function($scope, $http, $stateParams, $filter, postService, feedService, tagService, userPromise ){

            $scope.user = userPromise.data;
            $scope.posts = feedService.posts;
            


   
        }]);