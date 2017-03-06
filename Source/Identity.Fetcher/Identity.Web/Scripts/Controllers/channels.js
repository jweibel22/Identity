angular.module('inspire')
    .controller('ChannelsController', ['$scope', '$http', '$stateParams', '$modal', '$window', '$filter', 'userPromise', 'channelService', 'postService',
        function ($scope, $http, $stateParams, $modal, $window, $filter, userPromise, channelService, postService) {

            $scope.user = userPromise.data;
            $scope.filter = "";

            var treeInitialized = false;

            $scope.showMenu = function() {
                return $window.innerWidth > 1000;
            }

            function tagsEqual(n1, n2) {

                if (n1.nodes) {
                    for (var i = 0; i < n1.nodes.length; i++) {
                        if (!tagsEqual(n1.nodes[i], n2.nodes[i]))
                            return false;
                    }
                }

                var n1Ok = n1.tags && n1.tags.length == 1;
                var n2Ok = n2.tags && n2.tags.length == 1;

                if (!n1Ok && !n2Ok)
                    return true;
                else if (n1Ok && n2Ok)
                    return n1.tags[0] == n2.tags[0];
                else
                    return false;
           }

            function getUnreadCount(id) {
                var x = $filter('filter')($scope.user.Owns, { Id: id }, true)[0];
                if (x) {
                    return x.UnreadCount;
                } else {
                    return 0;
                }
            }

            function createTreeViewNode(channel) {

                var result = {
                    uid: channel.Id,
                    text: channel.Name,
                    href: "#/home/" + channel.Id
                };

                if (getUnreadCount(channel.Id) > 0) {
                    result.tags = [getUnreadCount(channel.Id)];
                }

                if (channel.Subscriptions.length > 0) {
                    result.nodes = channel.Subscriptions.map(createTreeViewNode);
                }

                return result;
            }

    
            // collapse and enable all before search //
            function reset(tree) {
                tree.collapseAll();
                tree.enableAll();
            }

            function collectUnrelated(nodes) {
                var unrelated = [];
                $.each(nodes, function (i, n) {
                    if (!n.searchResult && !n.state.expanded) { // no hit, no parent
                        unrelated.push(n);
                    }
                    if (!n.searchResult && n.nodes) { // recurse for non-result children
                        angular.extend(unrelated, collectUnrelated(n.nodes));
                    }
                });
                return unrelated;
            }

            var lastPattern = '';

            $scope.search = function () {
                var pattern = $scope.filter;
                if (pattern === lastPattern) {
                    return;
                }
                lastPattern = pattern;
                var tree = $('#tree').treeview(true);
                reset(tree);
                if (pattern.length < 3) { // avoid heavy operation
                    tree.clearSearch();
                } else {
                    tree.search(pattern);
                    // get all root nodes: node 0 who is assumed to be
                    //   a root node, and all siblings of node 0.
                    var roots = tree.getSiblings(0);
                    roots.push(tree.getNodes()[0]);

                    var unrelated = collectUnrelated(roots);
                    tree.disableNode(unrelated, { silent: true });
                }
            };


            $scope.$watch('user.Owns', function (newValue, oldValue, scope) {

                if (!treeInitialized)
                    return;

                var treeNodes = $('#tree').treeview('getNodes');

                for (var i = 0; i < treeNodes.length; i++) {
                    var node = treeNodes[i];

                    if (!node.parentId) {
                        
                        var x = $filter('filter')($scope.user.Owns, { Id: node.uid }, true)[0];

                        if (x) {
                            var newNode = createTreeViewNode(x);

                            if (!tagsEqual(newNode, node)) {
                                newNode.state = node.state; //TODO: copying of the state from old nodes to new nodes should be done recursively on all children
                                $('#tree').treeview('updateNode', [node, newNode, { silent: true }]);
                            }
                        }
                    }
                }
                
            }, true);


            $scope.$watchCollection('user.Owns', function (newArray) {
                console.log('collection changed');

                $scope.tree = $scope.user.ChannelMenuItems.map(createTreeViewNode);
                $('#tree').treeview({
                    data: $scope.tree,
                    levels: 1,
                    enableLinks: true,
                    showTags: true,

                    onNodeSelected: function (event, data) {
                        $window.location.href = data.href;
                    }
                });

                var allNodes = $('#tree').treeview('getNodes');
                $(allNodes).each(function (index, element) {

                    function handleDropEvent(event, ui) {
                        var dataType = ui.draggable.attr('data-type');

                        if (dataType === "post") {
                            postService.savePost2(element.uid, ui.draggable.attr('id'));
                        }
                        else if (dataType === "channel") {
                            channelService.addSubscription(element.uid, ui.draggable.attr('id'));
                        }                        
                    }

                    $(this.$el[0]).droppable({ drop: handleDropEvent, hoverClass: "drop-hover", });
                });

                treeInitialized = true;
            });

            $scope.totalUnreadCount = function(channel) {
                var result = channel.UnreadCount;

                for (var i = 0; i < channel.Subscriptions.length; i++) {
                    result += channel.Subscriptions[i].UnreadCount;
                }

                return result;
            }

        }]);