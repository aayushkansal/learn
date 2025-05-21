# gRPC API Reference

This document provides a reference for the gRPC services and methods available in this project, including sample JSON request payloads.

**General Instructions for gRPC Clients (e.g., Postman):**

1.  **Server URL**: You will need the server address and port where the gRPC service is hosted (e.g., `localhost:5000`). This information is typically part of your deployment configuration and is not found within this codebase.
2.  **Service Definition (`.proto` file)**: To call a gRPC service, your client will need access to its `.proto` definition file.
    *   For `MasterService`, the relevant file is `src/Services/Master/master.proto`.
    *   For other services (like `AlertService`, `HelpService`, etc.), their respective `.proto` files are located in their specific directories (e.g., `src/Services/Alert/alert.proto`).
3.  **Select Service & Method**: Once the `.proto` file is loaded into your client tool, you can select the desired Service and Method to call.
4.  **Request Message**: Provide the request data in JSON format as shown in the examples below.

---

## Service: `master.MasterService`

*Proto file location: `src/Services/Master/master.proto`*

This service manages roundtable data.

### Method: `GetStatus`

*   **Description:** Checks the operational status of the `MasterService`.
*   **Request Message:** `GetStatusRequest` (No fields)
*   **Sample JSON Request:**
    ```json
    {}
    ```

### Method: `GetRoundtable`

*   **Description:** Retrieves detailed information for a specific roundtable using its ID.
*   **Request Message:** `GetRoundtableRequest`
    *   `roundtable_id` (string, required): The unique identifier for the roundtable.
*   **Sample JSON Request:**
    ```json
    {
        "roundtable_id": "rt-master-1"
    }
    ```

### Method: `ListRoundtables`

*   **Description:** Lists available roundtables, with support for searching, status filtering, and pagination.
*   **Request Message:** `ListRoundtablesRequest`
    *   `search_query` (string, optional): Text to search in roundtable name, abbreviation, description, or associated user names.
    *   `status_filter` (enum `RoundtableStatus`, optional): Filters by status. Accepted values:
        *   `ROUNDTABLE_STATUS_UNSPECIFIED` (defaults to no filter or treated as `ALL`)
        *   `ACTIVE`
        *   `INACTIVE`
        *   `ALL`
    *   `page_size` (int32, optional): Number of results per page (default: 10, max: 100).
    *   `page_token` (string, optional): Token for pagination, typically representing an offset (e.g., "10" to skip the first 10 records).
*   **Sample JSON Request (List all active roundtables, default page size):**
    ```json
    {
        "status_filter": "ACTIVE"
    }
    ```
*   **Sample JSON Request (Search for "Alpha", page size 5):**
    ```json
    {
        "search_query": "Alpha",
        "page_size": 5
    }
    ```

### Method: `CreateRoundtable`

*   **Description:** Creates a new roundtable.
*   **Request Message:** `CreateRoundtableRequest`
    *   `name` (string, required): Name of the roundtable.
    *   `abbreviation` (string, required): A short, unique code or abbreviation for the roundtable.
    *   `is_active` (bool, required): Set to `true` for active status, `false` for inactive.
    *   `description` (string, optional): Detailed description of the roundtable.
    *   `client_ids` (repeated string, optional): List of client IDs to be associated with this roundtable.
    *   `director_user_ids` (repeated string, required): List of user IDs for the directors of this roundtable. At least one ID must be provided.
    *   `associate_user_ids` (repeated string, optional): List of user IDs for associates linked to this roundtable.
    *   `primary_director_user_id` (string, required): The user ID of the primary director. This ID must be included in the `director_user_ids` list.
    *   `primary_associate_user_id` (string, optional): The user ID of the primary associate. If provided, this ID must be included in the `associate_user_ids` list.
*   **Sample JSON Request:**
    ```json
    {
        "name": "New Finance Roundtable",
        "abbreviation": "NFR",
        "is_active": true,
        "description": "A brand new roundtable for finance discussions.",
        "client_ids": ["client-5", "client-6"],
        "director_user_ids": ["user-dir-3"],
        "associate_user_ids": ["user-assoc-4", "user-assoc-5"],
        "primary_director_user_id": "user-dir-3",
        "primary_associate_user_id": "user-assoc-4"
    }
    ```

---

## Standard `GetStatus` Method for Other Services

The following services each implement a standard `GetStatus` method.

*   **Services:**
    *   `advancesearch.AdvanceSearchService` (Proto: `src/Services/AdvanceSearch/advance_search.proto`)
    *   `alert.AlertService` (Proto: `src/Services/Alert/alert.proto`)
    *   `auriemmaexchange.AuriemmaExchangeService` (Proto: `src/Services/AuriemmaExchange/auriemma_exchange.proto`)
    *   `help.HelpService` (Proto: `src/Services/Help/help.proto`)
    *   `home.HomeService` (Proto: `src/Services/Home/home.proto`)
    *   `meeting.MeetingService` (Proto: `src/Services/Meeting/meeting.proto`)
    *   `survey.SurveyService` (Proto: `src/Services/Survey/survey.proto`)
    *   `tool.ToolService` (Proto: `src/Services/Tool/tool.proto`)
    *   `user.UserService` (Proto: `src/Services/User/user.proto`)

### Method: `GetStatus` (Common to services listed above)

*   **Description:** Checks the operational status of the respective service.
*   **Request Message:** `GetStatusRequest` (No fields)
*   **Sample JSON Request:**
    ```json
    {}
    ```

---

## Service: `community.CommunityService`

*Proto file location: `src/Services/Community/community.proto`*

*   This service currently has **no RPC methods defined**.

---
