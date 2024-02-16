(function () {

    "use strict";
    angular.module("app-labprofiles")
        .controller("CourseScheduledController", CourseScheduledController);

    CourseScheduledController.$inject = ['$scope', '$uibModalInstance', '$uibModal', '$http', 'svc', 'items', 'scheds', '$rootScope', '$route'];

    function CourseScheduledController($scope, $uibModalInstance, $uibModal, $http, svc, items, scheds, $rootScope, $route) {
        $scope.timeZoneList = [];
        $scope.timeRange = [];
        $scope.course = items;
        $scope.aCandidates = [];
        $scope.showError = true;
        $scope.bulkFile = "";
        var schedTime = {};
        $scope.allUsers = scheds;

        //$scope.pageChanged = function () {
        //    $scope.getAllUsers();
        //};

        $scope.totalItems = $scope.allUsers.length;
        $scope.currentPage = 1;
        $scope.itemsPerPage = 10;

        $scope.$watch('currentPage', function () {
            setPagingData($scope.currentPage);
        });

        $scope.$watch('deleteId', function () {
           
        });

        function setPagingData(page) {
            $scope.currentPage = page;
            var pagedData = scheds.slice((page - 1) * $scope.itemsPerPage, page * $scope.itemsPerPage);
            $scope.aCandidates = pagedData;
        }

        $scope.filterStatus = function (search) {
            $scope.temp = [];
            angular.forEach($scope.allUsers, function (value, key) {
                if (value.CourseEmail.toLowerCase().includes(search.toLowerCase()))
                    $scope.temp.push(value);
            });
            $scope.aCandidates = $scope.temp;
        }




        $scope.close = function () {
            $rootScope.$emit('reloadSched');
            $uibModalInstance.dismiss('close');
        };

        $scope.closeSched = function () {
            $rootScope.$emit('reloadSched');
            $uibModalInstance.dismiss('close');
        };

        $scope.picker = {
            date: null
        };


        $scope.startSubmittedDateOptions = {
            minDate: new Date(),
            maxDate: null
        },

        $scope.openCalendar = function (e, picker) {
            $scope.picker.open = true;
        };

        svc.getTimeZone().then(function (resp) {
            $scope.timeZoneList = resp;
        });

        svc.getListTimeRange().then(function (resp) {
            $scope.timeRange = resp;
        });



        $scope.order = ['b', 'a', 'c'];
        $scope.testDelim = [{ a: "sample@cloudswyft.com" }, { a: "sample1@cloudswyft.com" }, { a: "sample2@cloudswyft.com" }];

        $scope.value = new Date();
        $scope.width = "180px";

        $scope.uploadFiles = function (file, errFiles) {
            $scope.filename = file.name;
            $scope.showErrorChangePass = true;
            if (file !== null) {
                $scope.bulkFile = file;
                if (file.type === "text/csv" || file.type === "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
                    $scope.bulkFile = file;
                    $scope.showError = false;
                    $scope.showErrorChangePass = false;
                }
                else {
                    $scope.showError = true;
                    $scope.showErrorChangePass = true;
                    $scope.errorMsg = "Please select .csv or .xlsx file to upload";
                }
            }
        };


        $scope.save = function () {

            schedTime = {
                VEProfileID : parseInt($scope.courseName),
                TimeZone : $scope.timeZone,
                //StartTime: $scope.time,
                StartTime: $scope.picker.date,
                IdleTime: parseInt($scope.idleTime),
                ScheduledBy : currentUserId
            };

            var items = {
                title: "Course Schedule",
                message: "Are you sure you want to save?",
                type: "courseSchedule",
                isGrant: null,
                groupLabDetails: null,
                remainingCredits: null,
                schedTimes: schedTime
            };

            $uibModal.open({
                templateUrl: '/app/Common/ConfirmationModal.html',
                controller: "ConfirmationModalController",
                size: 'xs',
                backdrop: 'static',
                keyboard: true,
                resolve: {
                    items: function () {
                        return items;
                    },
                    userGroup: function () {
                        return 0;
                    },
                    VEProfileID: function () {
                        return 0;
                    },
                    userGrade: function () {
                        return null;
                    },
                    VEName: function () {
                        return null;
                    },
                    userid: function () {
                        return null;
                    },
                    resourceId: function () {
                        return $scope.bulkFile;
                    },
                    labhourdata: function () {
                        return null;
                    }

                }
            });


        };

        $scope.deleteSched = function (id) {
            svc.deleteTimeShedule(id).then(function (resp) {
                $scope.currentPage = $scope.currentPage;
                var pagedData = resp.slice(($scope.currentPage - 1) * $scope.itemsPerPage, $scope.currentPage * $scope.itemsPerPage);
                $scope.aCandidates = pagedData;
                $scope.allUsers = $scope.aCandidates;
            });
        }
    }

})();