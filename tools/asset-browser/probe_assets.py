import UnityPy, collections, os, sys

base = r"C:\Users\thana\Desktop\Durango Opencode\game\DurangoV2_Data"
targets = [
    os.path.join(base, "resources.assets"),
    os.path.join(base, "sharedassets0.assets"),
    os.path.join(base, "sharedassets2.assets"),
    os.path.join(base, "sharedassets3.assets"),
    os.path.join(base, "level3"),
]
for t in targets:
    if not os.path.exists(t):
        print(t, "MISSING")
        continue
    try:
        env = UnityPy.load(t)
    except Exception as e:
        print(t, "LOAD FAIL", e)
        continue
    c = collections.Counter(o.type.name for o in env.objects)
    print(os.path.basename(t), "total", len(env.objects))
    for k, v in c.most_common():
        print(f"   {k}: {v}")
