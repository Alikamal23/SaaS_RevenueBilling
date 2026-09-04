$(document).ready(function () {

    GetRoles_DDL();
    GetForms();

    $('#selectrole').on('change', function (e) {
        $('input:checkbox:checked').prop('checked', false);
        var selected_value = $(this).val();
        if (selected_value !== '') {
            GetMapping(selected_value);
        }
    });

    $('#SaveBtn').on('click', function () {
        UpdateRolesMapping();
    });


});
function GetRoles_DDL() {
    new APICALL(GetGlobalURL('Base', 'GetRoles'), 'GET', '', true).FETCH((result, error) => {

        if (result) {
            if (result.data != null) {
                $.each(result.data, function (i, option) {
                    $('#selectrole').append(
                        '<option value="' + option.RoleID + '">' + option.RoleName + '</option>'
                    );
                });
            }
        }
        if (error) {

            Swal.fire({
                icon: 'error',
                title: 'Error...',
                text: error.data.responseText,
                footer: ''
            });
        }
    });
}
function GetForms() {

    UTILITY.CheckSession((data_) => {

        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetForms'), 'GET', '', true).FETCH((result, error) => {

                if (result) {
                    if (result.data != null) {

                        var json = result.data.flatMap(x => {
                            var parentId = x.FormID;

                            // Build parent node
                            var parentNode = {
                                id: `form-${x.FormID}`,
                                parent: x.ParentFormID === 0 || x.ParentFormID === null ? "#" : `form-${x.ParentFormID}`,
                                text: x.FormDisplayName,
                                icon: x.iconclass ? x.iconclass : "jstree-folder"
                            };

                            // ✅ Only add children if this is NOT a root node
                            if (x.FormName === null) {
                                return [parentNode]; // return parent node only
                            }

                            // Build permission children
                            var children = [
                                { id: `view-${parentId}`, parent: parentNode.id, text: "View", icon: false, type: "perm-view" },
                                { id: `insert-${parentId}`, parent: parentNode.id, text: "Insert", icon: false, type: "perm-insert" },
                                { id: `update-${parentId}`, parent: parentNode.id, text: "Update", icon: false, type: "perm-update" },
                                { id: `delete-${parentId}`, parent: parentNode.id, text: "Delete", icon: false, type: "perm-delete" }
                            ];


                            return [parentNode, ...children];
                        });



                        $("#tree").jstree({
                            plugins: ["defaults", "checkbox"],
                            checkbox: {
                                three_state: true,
                                real_checkboxes: true,
                                checked_parent_open: true
                            },
                            core: {
                                data: json
                            },
                            types: {
                                "perm-view": { "icon": false },
                                "perm-insert": { "icon": false },
                                "perm-update": { "icon": false },
                                "perm-delete": { "icon": false }
                            }
                            
                        }).bind("loaded.jstree", function (event, data) {
                            $(this).jstree("open_all");
                        });

                        $("#tree").on("changed.jstree", function (e, data) {
                            if (!data.node) return;

                            let nodeId = data.node.id;

                            // Only trigger for "view" nodes
                            if (nodeId.startsWith("view-")) {
                                let formId = nodeId.split("-")[1];
                                let tree = $("#tree").jstree(true);

                                if (tree.is_checked(nodeId)) {
                                    // ✅ View is checked — do nothing (user may check others manually)
                                } else {
                                    // ❌ View is unchecked — uncheck insert/update/delete too
                                    tree.uncheck_node([`insert-${formId}`, `update-${formId}`, `delete-${formId}`]);
                                }
                            }
                        });

                    }
                }
                if (error) {

                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: error.data.responseText,
                        footer: ''
                    });
                }
            });
        }
    });
}
function GetMapping(RoleID) {
    UTILITY.CheckSession((data_) => {

        if (data_) {
            new APICALL(GetGlobalURL('Base', 'GetMapping_2?RoleId=' + RoleID), 'GET', '', true).FETCH((result, error) => {

                if (result) {
                    if (result.data != null) {

                        let checkedNodes = getPermissionNodeIds(result.data);

                        $("#tree").jstree(true).uncheck_all(); // clear old selection
                        $("#tree").jstree(true).check_node(checkedNodes);

                    }
                    
                }
                if (error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error...',
                        text: error.data.responseText,
                        footer: ''
                    });
                }
            });
        }
    });
}
function UpdateRolesMapping() {

    var roleId = $('#selectrole').val();

    if (roleId !== '') {
    
        let payload = buildRoleMapping(roleId);

        if (payload.RoleMappingCollection.length > 0) {

            Swal.fire({
                title: 'Do you want to save the changes?',
                showDenyButton: true,
                showCancelButton: false,
                confirmButtonText: 'Ok',
                denyButtonText: 'Cancel',
            }).then((result) => {
                if (result.isConfirmed) {
                    var Data = JSON.stringify(payload);
                    new APICALL(GetGlobalURL('Base', 'UpdateRolesMapping'), 'POST', Data, true).FETCH((result, error) => {

                        if (result) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Success...',
                                text: 'Forms Updated Successfully!',
                                footer: ''
                            });
                            var roleId = $('#selectrole').val();
                            GetMapping(roleId);
                        }
                        if (error) {

                            Swal.fire({
                                icon: 'error',
                                title: 'Error...',
                                text: error.data.responseText,
                                footer: ''
                            });
                        }
                    });
                }
            });
        }

    }
}
function getPermissionNodeIds(rolePermissions) {
    let checkedNodes = [];

    rolePermissions.forEach(p => {
        if (p.IsView) checkedNodes.push(`view-${p.FormID}`);
        if (p.AllowInsert) checkedNodes.push(`insert-${p.FormID}`);
        if (p.AllowUpdate) checkedNodes.push(`update-${p.FormID}`);
        if (p.AllowDelete) checkedNodes.push(`delete-${p.FormID}`);
    });

    return checkedNodes;
}

function buildRoleMapping(roleId) {
    // get all checked nodes
    let selectedNodes = $("#tree").jstree("get_checked", true);

    // prepare result dictionary keyed by FormID
    let roleMap = {};

    selectedNodes.forEach(node => {
        // node.id looks like: view-2, insert-3, update-4, delete-5
        let parts = node.id.split("-");
        let perm = parts[0];       // view | insert | update | delete
        let formId = parts[1];     // e.g., "2"

        // skip root form nodes (form-1 etc.)
        if (perm === "form") return;

        if (!roleMap[formId]) {
            roleMap[formId] = {
                FormsID: formId,
                AllowInsert: false,
                AllowUpdate: false,
                AllowDelete: false
                // (if you also want IsView, add: IsView: false)
            };
        }

        if (perm === "insert") roleMap[formId].AllowInsert = true;
        if (perm === "update") roleMap[formId].AllowUpdate = true;
        if (perm === "delete") roleMap[formId].AllowDelete = true;
        // if (perm === "view") roleMap[formId].IsView = true; // optional
    });

    return {
        RoleID: roleId,
        RoleMappingCollection: Object.values(roleMap)
    };
}

