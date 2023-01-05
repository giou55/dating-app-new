import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrl = environment.apiUrl;

  // we store members here, so they will be available to every component
  // without making a new request to database (caching data)
  members: Member[] = [];

  // for caching members we store the param queries as a key and the paginated results as a value
  // so for every call of getMembers method we first check the memberCache
  // if there are results for this param query
  memberCache = new Map(); // with map we can use get and set

  constructor(private http: HttpClient) {}

  getMembers(userParams: UserParams) {
    // Object.values(userParams) --> [18, 99, 1, 5, 'lastActive', 'male']
    // Object.values(userParams).join('-') --> "18-99-1-5-lastActive-male"

    // we get the response (value) for this query (key) from the map object
    const response = this.memberCache.get(Object.values(userParams).join('-'));

    // check if there is a value for that key
    if (response) return of(response);

    let params = this.getPaginationHeaders(
      userParams.pageNumber,
      userParams.pageSize
    );

    params = params.append('minAge', userParams.minAge);
    params = params.append('maxAge', userParams.maxAge);
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return this.getPaginatedResult<Member[]>(this.baseUrl + 'users', params).pipe(
      map(response => {
        // the response will the value for a key in the Map
        this.memberCache.set(Object.values(userParams).join('-'), response);
        // return response so that a component can use it 
        return response;
      })
    )
  }

  getMember(username: string) {
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.userName === username);
    
    // after reduce method the member variable will be an array like bellow,
    // and on every request the array will become larger,
    // and there will be duplicate users, but it does not matter

    // [{id: 12, userName: 'george', photoUrl: '85.jpg', age: 24, knownAs: 'george', …},
    //  {id: 1, userName: 'leigh', photoUrl: '37.jpg', age: 59, knownAs: 'Leigh', …},
    //  {id: 11, userName: 'bob', photoUrl: null, age: 37, knownAs: 'Bob', …},
    //  {id: 9, userName: 'todd', photoUrl: '51.jpg', age: 60, knownAs: 'Todd', …},
    //  {id: 1, userName: 'leigh', photoUrl: '37.jpg', age: 59, knownAs: 'Leigh', …},
    //  {id: 5, userName: 'kay', photoUrl: '74.jpg', age: 27, knownAs: 'Kay', …},
    //  {id: 3, userName: 'jenny', photoUrl: '77.jpg', age: 38, knownAs: 'Jenny', …}]

    // after find method we are finding the individual member from member array

    // if member is found we return it, else we will request to the API
    if (member) return of(member);

    // we do not need getHttpOptions method because we use jwt.interceptor to add Authorization header
    //return this.http.get<Member>(this.baseUrl + 'users/' + username, this.getHttpOptions())
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  // we do not need this because we use jwt.interceptor to add Authorization header
  // getHttpOptions(){
  //   const userString = localStorage.getItem('user');
  //   if (!userString) return;
  //   const user = JSON.parse(userString);
  //   return {
  //     headers: new HttpHeaders({
  //       Authorization: 'Bearer ' + user.token
  //     })
  //   }
  // }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      // nothing to return
      map(() => {
        const index = this.members.indexOf(member);
        // we update this specific member inside members array
        this.members[index] = { ...this.members[index], ...member };
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }

  // we made this generic, so the method will be reusable for many urls
  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    // we want to observe the response, and not just the body
    // and also we want to pass the params
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map((response) => {
        if (response.body) {
          paginatedResult.result = response.body;
        }
        const pagination = response.headers.get('Pagination');
        if (pagination) {
          paginatedResult.pagination = JSON.parse(pagination);
        }
        // we return because the component need this
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();
    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);
    return params;
  }
}
