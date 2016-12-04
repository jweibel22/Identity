angular.module('inspire')
    .controller('NavigatorCtrl', [
        '$scope', '$modal', '$state', '$window', '$location', 'authService', 'postService', 'channelService', 'userPromise', 
        function ($scope, $modal, $state, $window, $location, authService, postService, channelService, userPromise) {

            $scope.user = userPromise.data;
            $scope.state = $state;
            $scope.searchFor = "";

            $scope.newChannelWindowdata = {
                user: $scope.user,
                name: '',
                description: ''
            }


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


            $scope.newChannel = function () {

                $modal.open({
                    templateUrl: 'addNewChannel.html',
                    backdrop: true,
                    windowClass: 'modal',
                    controller: function ($scope, $modalInstance, windowdata) {
                        $scope.windowdata = windowdata;

                        $scope.submit = function () {

                            if (!$scope.windowdata.name || $scope.windowdata.name === '') {
                                $modalInstance.dismiss('cancel');
                                return;
                            }

                            channelService.create({ Name: $scope.windowdata.name }).success(function (data) {
                                $scope.windowdata.user.Owns.push(data);
                                $window.location.href = '#/home/' + data.Id;
                            });

                            $modalInstance.dismiss('cancel');
                        }
                        $scope.cancel = function () {
                            $modalInstance.dismiss('cancel');
                        };
                    },
                    resolve: {
                        windowdata: function () {
                            return $scope.newChannelWindowdata;
                        }
                    }
                });
            };
        }
    ]);
