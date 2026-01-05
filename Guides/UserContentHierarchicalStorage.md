# User content hierarchical storage

## Background

User content means basically the lua and screen content.

Contrary to regular dynamic properties those are not written in the postgresql database.

Instead a hash of the file content is stored in DB, and the data is stored in "data/user_content", and accessed directly by the client through nginx which servers that directory as static files.

## The problem

On big servers the number of files can grow to 100K+ which puts a strain on the filesystem.

## The solution

Starting from server 1.5.10 and client 1.4.12 one can opt in to instead store the user content hierarchically.

### Enabling hierarchical storage

Two changes need to be made to "config/dual.yaml":

```yaml
user_content:
    use_subdirs: true
    has_root_values: true
    migrate_from_root: true
http:
    user_content_cdn: "WHATEVER_YOU_HAVE|" # add a final pipe to current URL

```

The first part instruct the UserContent service to store hash "aabbccccc..." into "aa/bb/aabbccc....".

The second part tells the client to use hierarchical URLS from the given root instead of raw hash value.

### Dealing with existing data

#### Migrating

The simplest option is to migrate the data to new storage format using DualSQL (for docker version it is provided in the "python" image, sandbox service, for native version it is in "wincs/all" directory).

    /path/to/DualSQL config/dual.yaml --user-content-add-dirs

This command will copy each user content file from the flat path to the hierarchical path, then delete from the original location.

Note: this command will only work if dual.yaml "user_content" section is set as per sample above.


#### Alternate options: nginx trying both possible paths

If you do not wish to move existing data, you can instruct nginx to attempt both paths using a rule like:

```
location ~ ^/([0-9A-Fa-f]{2})/([0-9A-Fa-f]{2})/([0-9A-Fa-f]+)$ {
    root /path/to/files;

    # try nested first, then fall back to flat sha1
    try_files $uri /$3;
}
```