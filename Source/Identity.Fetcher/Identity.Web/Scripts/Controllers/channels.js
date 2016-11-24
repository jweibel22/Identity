angular.module('inspire')
    .controller('ChannelsController', ['$scope', '$http', '$stateParams', '$modal', '$window', 'userPromise', 'channelService',
        function ($scope, $http, $stateParams, $modal, $window, userPromise, channelService) {

            $scope.user = userPromise.data;

            $scope.newChannelWindowdata = {
                user: $scope.user,
                name: '',
                description: ''
            }

            $scope.totalUnreadCount = function(channel) {
                var result = channel.UnreadCount;

                for (var i = 0; i < channel.Subscriptions.length; i++) {
                    result += channel.Subscriptions[i].UnreadCount;
                }

                return result;
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
        }]);