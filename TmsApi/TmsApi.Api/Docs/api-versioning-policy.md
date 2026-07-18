# API Versioning Policy

## Purpose

This policy defines how the TMS API is versioned and how changes are communicated to API consumers. The goal is to provide a stable integration experience while allowing the API to evolve over time.

## Breaking Changes

A change is considered breaking if it can cause an existing client application to fail or behave differently without modification. Examples include:

* Removing an existing field from a response.
* Renaming a field.
* Changing an HTTP status code returned by an endpoint.
* Tightening validation rules that reject requests previously accepted.
* Changing the default sort order of returned data.

Breaking changes require a new API version.

## Non-Breaking (Additive) Changes

A change is considered non-breaking if existing clients continue to work without modification. Examples include:

* Adding a new optional field to a response.
* Adding a new endpoint.
* Adding a new optional query parameter.

These changes may be released within the current API version.

## Sunset Policy

When a new major version is released, the previous version will remain available for a minimum of **six months**. This allows training centres and partner organizations operating on quarterly maintenance schedules sufficient time to plan and complete migration activities.

## Communication Process

When a new major version is introduced:

* Deprecation, Sunset, and Link headers will be included in responses from the previous version.
* A CHANGELOG entry will document all relevant changes.
* An email notification will be sent to every team associated with an API key.
* A calendar invitation will be issued announcing the scheduled shutdown date of the previous version.

## API Version Selection

The primary API versioning strategy is **URL segment versioning**. This is the default approach because the version is visible in URLs, easier to discover, easier to document, and more useful during incident response and troubleshooting.

Example:

```text
GET /api/v1/courses
GET /api/v2/courses
```

Some partners, especially mobile clients that rely on cached CDN URLs, may require versioning without changing the URL. For these cases, the TMS API supports header-based versioning as an alternative.

Example:

```text
GET /api/courses
X-Api-Version: 2.0
```

Header-based versioning is **not enabled by default**. It is available only through **partner-by-partner opt-in** after reviewing the partner's technical requirements and integration needs.

ASP.NET supports multiple API version readers at the same time. The TMS API can resolve versions using both URL segments and headers. When a partner is approved for header-based versioning, requests using the `X-Api-Version` header will resolve to the requested API version even when the URL does not contain a version segment.

URL segment versioning remains the primary and recommended approach because it provides better visibility during monitoring, logging, and incident response.

## Version Migration

Clients are not required to migrate through every intermediate version. Direct upgrades are supported. For example, migration from **V1** directly to **V3** is allowed when V3 is available.

## Summary

If a change can cause an existing client to fail, it is a breaking change and requires a new API version. If existing clients continue to work without modification, the change is considered non-breaking and may be added to the current version.
