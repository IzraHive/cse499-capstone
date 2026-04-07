// All API calls use relative URL — same origin, cookie sent automatically
const API = '/api';

const PARISHES = {
  "Clarendon":     ["Clarendon Central","Clarendon North","Clarendon South East","Clarendon South West","Clarendon North West","Clarendon East","Clarendon North Central","Clarendon South"],
  "Hanover":       ["Hanover Eastern","Hanover Western"],
  "Kingston":      ["Kingston Central","Kingston East","Kingston West","Kingston & St. Andrew East","Kingston & St. Andrew North Central","Kingston & St. Andrew North East","Kingston & St. Andrew North West","Kingston & St. Andrew South","Kingston & St. Andrew South West","Kingston & St. Andrew West Central"],
  "Manchester":    ["Manchester Central","Manchester North East","Manchester North Western","Manchester Southern","Manchester West"],
  "Portland":      ["Portland Eastern","Portland Western"],
  "St. Andrew":    ["St. Andrew East Central","St. Andrew East Rural","St. Andrew North Central","St. Andrew North East","St. Andrew North West","St. Andrew South East","St. Andrew South West","St. Andrew West Central","St. Andrew West Rural"],
  "St. Ann":       ["St. Ann North East","St. Ann North West","St. Ann South East","St. Ann South West"],
  "St. Catherine": ["St. Catherine Central","St. Catherine East Central","St. Catherine East Rural","St. Catherine North Central","St. Catherine North East","St. Catherine North West","St. Catherine South","St. Catherine South East","St. Catherine South West","St. Catherine West Central"],
  "St. Elizabeth": ["St. Elizabeth North East","St. Elizabeth North West","St. Elizabeth South East","St. Elizabeth South West"],
  "St. James":     ["St. James Central","St. James East Central","St. James North West","St. James South East","St. James West Central"],
  "St. Mary":      ["St. Mary Central","St. Mary North East","St. Mary South East","St. Mary West"],
  "St. Thomas":    ["St. Thomas Eastern","St. Thomas Western"],
  "Trelawny":      ["Trelawny Northern","Trelawny Southern"],
  "Westmoreland":  ["Westmoreland Central","Westmoreland Eastern","Westmoreland Western"]
};

const GRANT_DESCRIPTIONS = {
  "Compassionate":   ["Funeral","Home Repair","Household Items","Medical"],
  "Emergency":       ["Fire","Natural Disaster"],
  "Entrepreneurial": ["Business"],
  "ESI":             ["Education"]
};

// No tokens — just store user info for display
function getUser()    { return JSON.parse(sessionStorage.getItem('gams_user') || 'null'); }
function getName()    { const u = getUser(); return u ? u.fullName : ''; }
function getRoles()   { const u = getUser(); return u ? u.roles : []; }
function isAdmin()    { return getRoles().includes('Admin'); }
function isSW()       { return getRoles().includes('SocialWorker'); }
function isFinance()  { return getRoles().includes('Finance'); }

// No Authorization header needed — cookie is sent automatically
function jsonHeaders() {
    return { 'Content-Type': 'application/json' };
}

function logout() {
    fetch(`${API}/auth/logout`, { method: 'POST', headers: jsonHeaders() })
        .finally(() => {
            sessionStorage.clear();
            window.location.href = '/index.html';
        });
}

// Check auth on every protected page
async function requireAuth() {
    try {
        const res = await fetch(`${API}/auth/me`);
        if (!res.ok) { window.location.href = '/index.html'; return null; }
        const user = await res.json();
        sessionStorage.setItem('gams_user', JSON.stringify(user));
        return user;
    } catch {
        window.location.href = '/index.html';
        return null;
    }
}

// Check auth and redirect admins/staff away from applicant pages
async function requireApplicant() {
    const user = await requireAuth();
    if (!user) return null;
    if (user.roles.includes('Admin') || user.roles.includes('SocialWorker') || user.roles.includes('Finance')) {
        window.location.href = '/admin.html';
        return null;
    }
    return user;
}

// Check auth and redirect applicants away from admin pages
async function requireStaff() {
    const user = await requireAuth();
    if (!user) return null;
    if (user.roles.includes('Applicant') && !user.roles.includes('Admin') && !user.roles.includes('SocialWorker') && !user.roles.includes('Finance')) {
        window.location.href = '/dashboard.html';
        return null;
    }
    return user;
}