angular.module('inspire')
    .controller('NavigatorCtrl', [
        '$scope', '$modal', '$state', '$window', '$location', 'authService', 'postService', 'channelService', 'userPromise', 
        function ($scope, $modal, $state, $window, $location, authService, postService, channelService, userPromise) {

            $scope.user = userPromise.data;
            $scope.state = $state;
            $scope.searchFor = "";

            //var guid = function() {
            //    function _p8(s) {
            //        var p = (Math.random().toString(16)+"000000000").substr(2,8);
            //        return s ? "-" + p.substr(0,4) + "-" + p.substr(4,4) : p ;
            //    }
            //    return _p8() + _p8(true) + _p8(true) + _p8();
            //}

            $scope.logOut = function () {
                authService.logOut();
                $location.replace().path('/#/home');
            }

            $scope.authentication = authService.authentication;

            $scope.search = function(event) {
                var url = "#/searchChannelOrUser?query=" + $scope.searchFor;
                $scope.searchFor = "";
                $window.location.href = url;
            }
        }
    ]);
