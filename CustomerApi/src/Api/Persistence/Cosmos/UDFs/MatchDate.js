// DO_NOT_REMOVE version:1
// any change to this file requires a manual increment of the version
function userDefinedFunction(d, f) {
    const date = new Date(d).setHours(0, 0, 0, 0);
    const filterDate = new Date(f).setHours(0, 0, 0, 0);
    return date === filterDate;
}
