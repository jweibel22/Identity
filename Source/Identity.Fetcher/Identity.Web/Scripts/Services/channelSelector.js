angular.module('inspire')
        .factory('channelSelectorService', ['$http', '$modal', function ($http, $modal) {

        var o = {
            selectedChannel: ''
        };
            

            o.selectChannel = function (channels, channelSelected) {

                o.channels = channels;

                $modal.open({
                    templateUrl: 'channelSelector.html',
                    backdrop: true,
                    windowClass: 'modal',
                    controller: function ($scope, $modalInstance, windowdata) {
                        $scope.windowdata = windowdata;

                        $scope.submit = function () {

                            if (!$scope.windowdata.selectedChannel || $scope.windowdata.selectedChannel === '') {
                                $modalInstance.dismiss('cancel');
                                return;
                            }

                            channelSelected($scope.windowdata.selectedChannel.Id);

                            $modalInstance.dismiss('cancel');
                        }
                        $scope.cancel = function () {
                            $modalInstance.dismiss('cancel');
                        };
                    },
                    resolve: {
                        windowdata: function () {
                            return o;
                        }
                    }
                });
            }

        return o;
    }
]);