angular.module('inspire')
    .controller('SearchController', [
        '$scope', '$http', '$stateParams', 'postService', 'userPromise','channelService','userService',
        function($scope, $http, $stateParams, postService, userPromise, channelService, userService ){

            $scope.query = $stateParams.query;
            $scope.user = userPromise.data;
            $scope.posts = postService.queryresults;
            $scope.channels = channelService.searchResult;
            $scope.users = userService.searchResult;
        }]);