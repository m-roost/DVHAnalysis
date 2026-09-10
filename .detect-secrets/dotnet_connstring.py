"""
Custom detect-secrets plugins for this repository.

They flag the parts of an ADO.NET / SQL Server connection string that should
not be committed. The built-in KeywordDetector misses these in .config files
because it requires a quote character right after the equals sign, and
connection strings do not quote their values.

  DotNetConnectionStringDetector          keys "password", "pwd"            (a real secret)
  DotNetConnectionStringServerDetector    keys "data source", "server"      (system information)
  DotNetConnectionStringDatabaseDetector  keys "initial catalog", "database"
  DotNetConnectionStringUserDetector      keys "user id", "uid", "user"

Loaded via the relative "file://" entries in .secrets.baseline (see
"plugins_used"). Do not add "--plugin" to the pre-commit hook args, or
detect-secrets will rewrite those entries as absolute paths on the next
baseline update and break the hook on other machines.
"""
import re

from detect_secrets.plugins.base import RegexBasedDetector


def _connection_string_key(*keys):
    """
    Regex for one "key=value" pair of a connection string.

    To avoid matching ordinary code such as `server = new Foo();`, the key must
    directly follow a ';' or an opening quote (or start the line), and the value
    must run up to a ';', a closing quote, or the end of the line. Parentheses
    are excluded from the value so that `database = OpenDb();` is not matched;
    this also skips local placeholders such as `(local)` and `(localdb)`.
    """
    return re.compile(
        r'(?i)(?:^|[;"\'])\s*(?:' + '|'.join(keys) + r')\s*=\s*([^;"\'\s()]+)(?=\s*(?:[;"\']|$))',
    )


class DotNetConnectionStringDetector(RegexBasedDetector):
    """Password inside a connection string."""

    secret_type = 'Connection String Password'  # pragma: allowlist secret

    denylist = [
        re.compile(r'(?i)(?:password|pwd)\s*=\s*([^;"\'\s]+)'),  # pragma: allowlist secret
    ]


class DotNetConnectionStringServerDetector(RegexBasedDetector):
    """Server / host name inside a connection string."""

    secret_type = 'Connection String Server'  # pragma: allowlist secret

    denylist = [
        _connection_string_key('data source', 'server', 'address', 'addr', 'network address'),
    ]


class DotNetConnectionStringDatabaseDetector(RegexBasedDetector):
    """Database name inside a connection string."""

    secret_type = 'Connection String Database'  # pragma: allowlist secret

    denylist = [
        _connection_string_key('initial catalog', 'database'),
    ]


class DotNetConnectionStringUserDetector(RegexBasedDetector):
    """User name inside a connection string."""

    secret_type = 'Connection String User Id'  # pragma: allowlist secret

    denylist = [
        _connection_string_key('user id', 'uid', 'user'),
    ]
