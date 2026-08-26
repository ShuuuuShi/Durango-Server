import socket, json, time, urllib.request, urllib.parse, sys

MB_HOST = "127.0.0.1"
MB_PORT = 8193
GW = "http://127.0.0.1:8190"
ENTITY_ID = "ce96e22b-42d6-4ab2-97f3-f1db9ec9f40b"

def mb(obj, timeout=5):
    s = socket.create_connection((MB_HOST, MB_PORT), timeout)
    s.settimeout(timeout)
    s.sendall((json.dumps(obj) + "\n").encode())
    try:
        line = s.recv(262144).decode(errors='replace').strip()
    finally:
        s.close()
    try:
        return json.loads(line)
    except Exception:
        return {"_raw": line}

def read(path):
    return mb({"request_id": "t-" + str(int(time.time()*1000)), "op": "read", "path": path})

def cmd(name, **kw):
    o = {"request_id": "t-" + str(int(time.time()*1000)), "op": "command", "name": name}
    o.update(kw)
    return mb(o)

def cheat(command):
    data = urllib.parse.urlencode({"entity_id": ENTITY_ID, "command": command}).encode()
    req = urllib.request.Request(GW + "/admin/cheat", data=data, method="POST")
    try:
        with urllib.request.urlopen(req, timeout=10) as r:
            return json.loads(r.read().decode())
    except Exception as e:
        return {"_error": str(e)}

def players():
    try:
        with urllib.request.urlopen(GW + "/admin/players", timeout=10) as r:
            return json.loads(r.read().decode())
    except Exception as e:
        return {"_error": str(e)}

def field(resp, *keys):
    cur = resp
    for k in keys:
        if not isinstance(cur, dict):
            return None
        cur = cur.get(k)
    return cur

def data(resp):
    return field(resp, "data", "data")

def show(resp):
    d = data(resp)
    if d is None:
        return json.dumps(resp, ensure_ascii=False)
    return json.dumps(d, ensure_ascii=False)
