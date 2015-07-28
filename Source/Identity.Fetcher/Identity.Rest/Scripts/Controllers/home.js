angular.module('inspire')
.controller('MainCtrl', [
    '$scope', '$modal', '$http', '$stateParams', '$location', '$filter', 'postService', 'channelService', 'tagService', 'channelPromise', 'userPromise', 
    function($scope, $modal, $http, $stateParams, $location, $filter, postService, channelService, tagService, channelPromise, userPromise){

        $scope.user = userPromise.data;        
        $scope.posts = postService.posts[channelPromise.data.Id];

        //we want the $scope.channel to point to the same channel object that is used in other controllers
        $scope.channel = $filter('filter')(channelService.channels, { Id: channelPromise.data.Id }, true)[0];
        $scope.channel.TagCloud = channelPromise.data.TagCloud; //TODO: find a better way!

        $scope.channelFollowed = $filter('filter')($scope.user.FollowsChannels, {Id: $scope.channel.Id}, true).length > 0;
        $scope.channelOwned = $filter('filter')($scope.user.Owns, { Id: $scope.channel.Id }, true).length > 0;
        $scope.showOnlyUnread = true;

        for (var i = 0; i < $scope.channel.TagCloud.length; i++) {
            $scope.channel.TagCloud[i].link = "#/search?query=" + $scope.channel.TagCloud[i].text;
        }

        $scope.subscribe = function () {
            channelService.subscribe($scope.channel.Id).success(function (data) {
                $scope.channelFollowed = true;
            });
        };

        $scope.unsubscribe = function () {
            channelService.unsubscribe($scope.channel.Id).success(function (data) {
                console.log("done");
                $scope.channelFollowed = false;
            });
        };


        $scope.showOnlyUnreadChanged = function() {

            postService.getFromChannel($scope.channel.Id, $scope.showOnlyUnread).success(function(data) {
                $scope.posts = postService.posts[$scope.channel.Id];
            });
        }

    }]);

angular.module('inspire').directive("scroll", function ($window) {

    function checkVisible(elm) {
            var vpH = $(window).height(), // Viewport Height
            st = $(window).scrollTop(), // Scroll Top
            y = $(elm).offset().top,
            elementHeight = $(elm).height();
        
            var res = ((y < (vpH + st)) && (st < (y + elementHeight)));
            return res;                
    };

    return function (scope, element, attrs) {
        angular.element($window).bind("scroll", function () {
            
            var divs = $(element).find('div.readable');

            for (var i = 1; i < divs.length; i++) {
                if (checkVisible(divs[i])) {
                    scope.read(scope.posts[i-1]);                    
                }
            }

            scope.$apply();
        });
    };
});