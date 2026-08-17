#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 4 ]]; then
  echo "Usage: $0 <publish-dir> <output-dir> <version> <deb-architecture>" >&2
  exit 2
fi

publish_dir="$(cd "$1" && pwd)"
mkdir -p "$2"
output_dir="$(cd "$2" && pwd)"
if [[ "$output_dir" == "/" ]]; then
  echo "Refusing to use the filesystem root as output directory." >&2
  exit 2
fi
version="$3"
deb_architecture="$4"
package_name="exifrenamer"
staging_dir="$output_dir/deb-root"

rm -rf "$staging_dir"
mkdir -p \
  "$staging_dir/DEBIAN" \
  "$staging_dir/opt/exifrenamer" \
  "$staging_dir/usr/bin" \
  "$staging_dir/usr/share/applications" \
  "$staging_dir/usr/share/pixmaps"

cp -R "$publish_dir"/. "$staging_dir/opt/exifrenamer/"
chmod 0755 "$staging_dir/opt/exifrenamer/ExifRenamer"
ln -s /opt/exifrenamer/ExifRenamer "$staging_dir/usr/bin/exifrenamer"
cp ExifRenamer/Assets/ExifRenamer.ico "$staging_dir/usr/share/pixmaps/exifrenamer.ico"

installed_size="$(du -sk "$staging_dir/opt/exifrenamer" | cut -f1)"
cat > "$staging_dir/DEBIAN/control" <<EOF
Package: $package_name
Version: $version
Section: graphics
Priority: optional
Architecture: $deb_architecture
Installed-Size: $installed_size
Maintainer: 7echnet
Depends: libc6, libgcc-s1 | libgcc1, libstdc++6, zlib1g, libssl3 | libssl1.1, libgssapi-krb5-2, libfontconfig1, libx11-6, libice6, libsm6
Description: Rename files using their EXIF metadata
 ExifRenamer is a cross-platform graphical application for previewing and
 applying file names derived from EXIF metadata.
EOF

cat > "$staging_dir/usr/share/applications/exifrenamer.desktop" <<'EOF'
[Desktop Entry]
Type=Application
Name=ExifRenamer
Comment=Rename files using their EXIF metadata
Exec=/opt/exifrenamer/ExifRenamer
Icon=/usr/share/pixmaps/exifrenamer.ico
Terminal=false
Categories=Graphics;Utility;
EOF

chmod 0755 "$staging_dir/DEBIAN"
chmod 0644 "$staging_dir/DEBIAN/control"
chmod 0644 "$staging_dir/usr/share/applications/exifrenamer.desktop"

deb_path="$output_dir/ExifRenamer-linux-x64.deb"
archive_path="$output_dir/ExifRenamer-linux-x64.tar.gz"
dpkg-deb --root-owner-group --build "$staging_dir" "$deb_path"
tar -C "$publish_dir" -czf "$archive_path" .
rm -rf "$staging_dir"

echo "Created $deb_path"
echo "Created $archive_path"
